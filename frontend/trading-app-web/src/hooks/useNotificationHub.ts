import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../store/auth';
import type { NotificationPayload } from '../types/api';

/**
 * SignalR /hubs/notifications — JWT ile auth.
 * Kullanıcı bazlı canlı bildirimler.
 */
export function useNotificationHub(onNotification?: (n: NotificationPayload) => void) {
  const token = useAuthStore((s) => s.accessToken);
  const [connected, setConnected] = useState(false);
  const connRef = useRef<signalR.HubConnection | null>(null);
  const callbackRef = useRef(onNotification);
  callbackRef.current = onNotification;

  useEffect(() => {
    if (!token) return;
    if (connRef.current) return;

    const conn = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    conn.on('NotificationReceived', (n: NotificationPayload) => {
      callbackRef.current?.(n);
    });

    conn.onreconnecting(() => setConnected(false));
    conn.onreconnected(() => setConnected(true));
    conn.onclose(() => setConnected(false));

    connRef.current = conn;

    conn.start()
      .then(() => setConnected(true))
      .catch((err) => {
        console.error('Notification hub connect error:', err);
        connRef.current = null;
      });

    return () => {
      const c = connRef.current;
      if (c) {
        connRef.current = null;
        c.stop().catch(() => {});
      }
    };
  }, [token]);

  return { connected };
}