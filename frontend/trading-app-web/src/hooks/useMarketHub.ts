import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export interface PriceUpdate {
  symbol: string;
  price: number;
  changePercent: number;
  dailyVolume: number;
  timestampUtc: string;
}

/**
 * SignalR /hubs/market bağlantısı.
 * Vite proxy üzerinden WebSocket direkt kullanılır (negotiate yok).
 */
export function useMarketHub(onUpdate?: (u: PriceUpdate) => void) {
  const [connected, setConnected] = useState(false);
  const connRef = useRef<signalR.HubConnection | null>(null);
  const callbackRef = useRef(onUpdate);
  callbackRef.current = onUpdate;

  useEffect(() => {
    // Guard: StrictMode'da 2. çalıştırmayı engelle
    if (connRef.current) return;

    const conn = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/market', {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    conn.on('MarketPriceUpdated', (u: PriceUpdate) => {
      callbackRef.current?.(u);
    });

    conn.onreconnecting(() => setConnected(false));
    conn.onreconnected(() => setConnected(true));
    conn.onclose(() => setConnected(false));

    connRef.current = conn;

    conn.start()
      .then(() => setConnected(true))
      .catch((err) => {
        console.error('SignalR connect error:', err);
        connRef.current = null; // başarısızsa temizle, retry'a izin ver
      });

    return () => {
      // StrictMode cleanup — sadece unmount'ta durdur
      const c = connRef.current;
      if (c) {
        connRef.current = null;
        c.stop().catch(() => {});
      }
    };
  }, []);

  return { connected };
}