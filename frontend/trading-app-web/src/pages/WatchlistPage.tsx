import { useEffect, useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useAuthStore } from '../store/auth';
import { api } from '../lib/api';
import type { WatchlistItemDto } from '../types/api';
import { useMarketHub, type PriceUpdate } from '../hooks/useMarketHub';

const fmt = (n: number, d = 2) =>
  n.toLocaleString('tr-TR', { minimumFractionDigits: d, maximumFractionDigits: d });

export default function WatchlistPage() {
  const token = useAuthStore((s) => s.accessToken);
  const [items, setItems] = useState<WatchlistItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [newSymbol, setNewSymbol] = useState('');
  const [error, setError] = useState<string | null>(null);

  // Canlı fiyat güncellemelerini dinle
  const onUpdate = useCallback((u: PriceUpdate) => {
    setItems((prev) =>
      prev.map((it) =>
        it.symbol === u.symbol
          ? { ...it, currentPrice: u.price, changePercent: u.changePercent }
          : it
      )
    );
  }, []);
  const { connected } = useMarketHub(onUpdate);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const data = await api.get<WatchlistItemDto[]>('/api/watchlist', token);
      setItems(data || []);
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => { load(); }, [load]);

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    const sym = newSymbol.trim().toUpperCase();
    if (!sym) return;
    try {
      await api.post(`/api/watchlist/${sym}`, undefined, token);
      setNewSymbol('');
      await load();
    } catch (e: any) {
      setError(e.apiMessage || 'Eklenemedi');
    }
  };

  const handleRemove = async (assetId: string) => {
    try {
      await api.delete(`/api/watchlist/${assetId}`, token);
      setItems((prev) => prev.filter((i) => i.assetId !== assetId));
    } catch (e: any) {
      setError(e.apiMessage || 'Silinemedi');
    }
  };

  return (
    <div className="p-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">İzleme Listesi</h1>
          <p className="text-sm text-[#8b9bb4] mt-1">{items.length} enstrüman</p>
        </div>
        <div className="flex items-center gap-2">
          <span className={`w-2 h-2 rounded-full ${connected ? 'bg-[#4ade80] animate-pulse' : 'bg-[#f87171]'}`} />
          <span className="text-xs text-[#8b9bb4]">{connected ? 'Canlı' : 'Bağlantı yok'}</span>
        </div>
      </div>

      {/* Ekleme formu */}
      <form onSubmit={handleAdd} className="flex gap-3">
        <input
          type="text"
          placeholder="Sembol ekle (örn. THYAO)"
          value={newSymbol}
          onChange={(e) => setNewSymbol(e.target.value.toUpperCase())}
          className="w-64 px-4 py-2 rounded-lg bg-[#111a2e] border border-[#1f2a44] text-white focus:border-[#4fc3f7] focus:outline-none"
        />
        <button
          type="submit"
          className="px-5 py-2 rounded-lg bg-gradient-to-r from-[#4fc3f7] to-[#7c4dff] text-white text-sm font-semibold"
        >
          Ekle
        </button>
      </form>

      {error && (
        <div className="px-3 py-2 rounded-lg bg-[#2a1a1a] text-[#f87171] text-sm">{error}</div>
      )}

      {loading ? (
        <div className="text-[#8b9bb4]">Yükleniyor...</div>
      ) : items.length === 0 ? (
        <div className="text-center py-16 text-[#8b9bb4]">
          <div className="text-lg mb-2">Liste boş</div>
          <div className="text-sm">Yukarıdan bir sembol ekle (THYAO, BTC, AAPL...)</div>
        </div>
      ) : (
        <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg overflow-hidden">
          <table className="w-full">
            <thead className="bg-[#0f1729]">
              <tr className="text-xs text-[#8b9bb4] uppercase">
                <th className="px-5 py-3 text-left">Sembol</th>
                <th className="px-5 py-3 text-left">İsim</th>
                <th className="px-5 py-3 text-right">Fiyat</th>
                <th className="px-5 py-3 text-right">Değişim</th>
                <th className="px-5 py-3 text-right">Tip</th>
                <th className="px-5 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {items.map((it) => (
                <tr key={it.assetId} className="border-t border-[#131c30] hover:bg-[#172239]">
                  <td className="px-5 py-3 text-sm font-semibold">
                    <Link to={`/trading/${it.symbol}`} className="text-white hover:text-[#4fc3f7]">
                      {it.symbol}
                    </Link>
                  </td>
                  <td className="px-5 py-3 text-sm text-[#8b9bb4] truncate max-w-[300px]">{it.name}</td>
                  <td className="px-5 py-3 text-sm num text-right text-white">{fmt(it.currentPrice, 4)}</td>
                  <td className={`px-5 py-3 text-sm num text-right ${it.changePercent >= 0 ? 'text-[#4ade80]' : 'text-[#f87171]'}`}>
                    {it.changePercent >= 0 ? '+' : ''}{fmt(it.changePercent)}%
                  </td>
                  <td className="px-5 py-3 text-right">
                    <span className="text-xs px-2 py-0.5 rounded bg-[#1f2a44] text-[#8b9bb4]">{it.assetType}</span>
                  </td>
                  <td className="px-5 py-3 text-right">
                    <button
                      onClick={() => handleRemove(it.assetId)}
                      className="text-xs text-[#f87171] hover:underline"
                    >
                      Kaldır
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}