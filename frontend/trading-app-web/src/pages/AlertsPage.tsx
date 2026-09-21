import { useEffect, useState } from 'react';
import { useAuthStore } from '../store/auth';
import { api } from '../lib/api';
import type { PriceAlertDto } from '../types/api';

const fmt = (n: number, d = 2) =>
  n.toLocaleString('tr-TR', { minimumFractionDigits: d, maximumFractionDigits: d });

export default function AlertsPage() {
  const token = useAuthStore((s) => s.accessToken);
  const [alerts, setAlerts] = useState<PriceAlertDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [symbol, setSymbol] = useState('THYAO');
  const [condition, setCondition] = useState(1); // 1 = GreaterThan, 2 = LessThan
  const [targetPrice, setTargetPrice] = useState('400');
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    setLoading(true);
    try {
      const data = await api.get<PriceAlertDto[]>('/api/price-alerts', token);
      setAlerts(data || []);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [token]);

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await api.post('/api/price-alerts', {
        symbol: symbol.trim().toUpperCase(),
        condition,
        targetPrice: parseFloat(targetPrice),
      }, token);
      await load();
    } catch (e: any) {
      setError(e.apiMessage || 'Oluşturulamadı');
    }
  };

  const handleDelete = async (id: string) => {
    await api.delete(`/api/price-alerts/${id}`, token);
    setAlerts((prev) => prev.filter((a) => a.id !== id));
  };

  const inputClass = "px-3 py-2 rounded-lg bg-[#0b1220] border border-[#1f2a44] text-white focus:border-[#4fc3f7] focus:outline-none";

  return (
    <div className="p-8 space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-white">Fiyat Alarmları</h1>
        <p className="text-sm text-[#8b9bb4] mt-1">{alerts.length} alarm</p>
      </div>

      {/* Yeni alarm */}
      <form onSubmit={handleAdd} className="flex flex-wrap gap-3 items-end">
        <div>
          <label className="block text-xs text-[#8b9bb4] mb-1">Sembol</label>
          <input value={symbol} onChange={(e) => setSymbol(e.target.value.toUpperCase())} className={`${inputClass} w-32`} />
        </div>
        <div>
          <label className="block text-xs text-[#8b9bb4] mb-1">Koşul</label>
          <select value={condition} onChange={(e) => setCondition(parseInt(e.target.value))} className={`${inputClass} w-40`}>
            <option value={1}>Fiyat üzerine çıkınca ( &gt; )</option>
            <option value={2}>Fiyat altına düşünce ( &lt; )</option>
          </select>
        </div>
        <div>
          <label className="block text-xs text-[#8b9bb4] mb-1">Hedef Fiyat</label>
          <input type="number" step="any" value={targetPrice} onChange={(e) => setTargetPrice(e.target.value)} className={`${inputClass} w-32`} />
        </div>
        <button type="submit" className="px-5 py-2 rounded-lg bg-gradient-to-r from-[#4fc3f7] to-[#7c4dff] text-white text-sm font-semibold">
          Alarm Kur
        </button>
      </form>

      {error && <div className="px-3 py-2 rounded-lg bg-[#2a1a1a] text-[#f87171] text-sm">{error}</div>}

      {loading ? (
        <div className="text-[#8b9bb4]">Yükleniyor...</div>
      ) : alerts.length === 0 ? (
        <div className="text-center py-16 text-[#8b9bb4]">
          <div className="text-lg mb-2">Henüz alarm yok</div>
          <div className="text-sm">Örn: BTC 100.000 USD üzerine çıkınca haber ver</div>
        </div>
      ) : (
        <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg overflow-hidden">
          <table className="w-full">
            <thead className="bg-[#0f1729]">
              <tr className="text-xs text-[#8b9bb4] uppercase">
                <th className="px-5 py-3 text-left">Sembol</th>
                <th className="px-5 py-3 text-left">Koşul</th>
                <th className="px-5 py-3 text-right">Hedef</th>
                <th className="px-5 py-3 text-center">Durum</th>
                <th className="px-5 py-3 text-left">Tarih</th>
                <th className="px-5 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {alerts.map((a) => (
                <tr key={a.id} className="border-t border-[#131c30] hover:bg-[#172239]">
                  <td className="px-5 py-3 text-sm font-semibold text-white">{a.symbol}</td>
                  <td className="px-5 py-3 text-sm text-[#8b9bb4]">
                    {a.condition === 'GreaterThan' ? '>' : '<'}
                  </td>
                  <td className="px-5 py-3 text-sm num text-right text-white">{fmt(a.targetPrice, 4)}</td>
                  <td className="px-5 py-3 text-center">
                    {a.isActive ? (
                      <span className="text-xs px-2 py-0.5 rounded bg-[#0d2f1f] text-[#4ade80]">Aktif</span>
                    ) : (
                      <span className="text-xs px-2 py-0.5 rounded bg-[#2f0d0d] text-[#f87171]">
                        Tetiklendi
                      </span>
                    )}
                  </td>
                  <td className="px-5 py-3 text-xs text-[#8b9bb4]">
                    {new Date(a.createdAt).toLocaleString('tr-TR')}
                  </td>
                  <td className="px-5 py-3 text-right">
                    <button onClick={() => handleDelete(a.id)} className="text-xs text-[#f87171] hover:underline">Sil</button>
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