import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuthStore } from '../store/auth';
import { api } from '../lib/api';
import type { AssetDto } from '../types/api';

const fmt = (n: number, d = 2) =>
  n.toLocaleString('tr-TR', { minimumFractionDigits: d, maximumFractionDigits: d });

export default function MarketsPage() {
  const token = useAuthStore((s) => s.accessToken);
  const [assets, setAssets] = useState<AssetDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState('');
  const [typeFilter, setTypeFilter] = useState<string>('All');
  const [sortKey, setSortKey] = useState<'symbol' | 'price' | 'change' | 'volume'>('change');
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('desc');

  useEffect(() => {
    if (!token) return;
    api.get<AssetDto[]>('/api/assets', token)
      .then((a) => setAssets(a || []))
      .finally(() => setLoading(false));
  }, [token]);

  const filtered = useMemo(() => {
    const q = query.trim().toUpperCase();
    let list = assets;

    if (q) {
      list = list.filter(
        (a) => a.symbol.toUpperCase().includes(q) || a.name.toUpperCase().includes(q)
      );
    }

    if (typeFilter !== 'All') {
      list = list.filter((a) => a.assetType === typeFilter);
    }

    const dir = sortDir === 'asc' ? 1 : -1;
    list = [...list].sort((a, b) => {
      switch (sortKey) {
        case 'symbol': return dir * a.symbol.localeCompare(b.symbol);
        case 'price':  return dir * (a.currentPrice - b.currentPrice);
        case 'change': return dir * (a.changePercent - b.changePercent);
        case 'volume': return dir * (a.dailyVolume - b.dailyVolume);
      }
    });

    return list;
  }, [assets, query, typeFilter, sortKey, sortDir]);

  const toggleSort = (key: typeof sortKey) => {
    if (sortKey === key) setSortDir(sortDir === 'asc' ? 'desc' : 'asc');
    else { setSortKey(key); setSortDir('desc'); }
  };

  if (loading) return <div className="p-8 text-[#8b9bb4]">Yükleniyor...</div>;

  const arrow = (key: typeof sortKey) =>
    sortKey === key ? (sortDir === 'asc' ? ' ↑' : ' ↓') : '';

  return (
    <div className="p-8 space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-white">Piyasalar</h1>
        <p className="text-sm text-[#8b9bb4] mt-1">{filtered.length} / {assets.length} enstrüman</p>
      </div>

      {/* Filtreler */}
      <div className="flex flex-wrap gap-3 items-center">
        <input
          type="text"
          placeholder="Hisse ara (örn. THYAO, Aselsan)..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          className="w-80 px-4 py-2 rounded-lg bg-[#111a2e] border border-[#1f2a44] text-white focus:border-[#4fc3f7] focus:outline-none"
        />

        <div className="flex gap-1 bg-[#111a2e] border border-[#1f2a44] rounded-lg p-1">
          {['All', 'Stock', 'Crypto', 'Etf', 'Forex'].map((t) => (
            <button
              key={t}
              onClick={() => setTypeFilter(t)}
              className={`px-3 py-1 text-xs rounded-md transition ${
                typeFilter === t ? 'bg-[#1d2b48] text-white' : 'text-[#8b9bb4] hover:text-white'
              }`}
            >
              {t === 'All' ? 'Tümü' : t === 'Stock' ? 'Hisse' : t === 'Crypto' ? 'Kripto' : t}
            </button>
          ))}
        </div>
      </div>

      {/* Tablo */}
      <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg overflow-hidden">
        <div className="max-h-[calc(100vh-280px)] overflow-y-auto">
          <table className="w-full">
            <thead className="bg-[#0f1729] sticky top-0">
              <tr className="text-xs text-[#8b9bb4] uppercase">
                <th onClick={() => toggleSort('symbol')} className="px-5 py-3 text-left cursor-pointer hover:text-white">Sembol{arrow('symbol')}</th>
                <th className="px-5 py-3 text-left">İsim</th>
                <th onClick={() => toggleSort('price')} className="px-5 py-3 text-right cursor-pointer hover:text-white">Fiyat{arrow('price')}</th>
                <th onClick={() => toggleSort('change')} className="px-5 py-3 text-right cursor-pointer hover:text-white">Değişim{arrow('change')}</th>
                <th onClick={() => toggleSort('volume')} className="px-5 py-3 text-right cursor-pointer hover:text-white">Hacim{arrow('volume')}</th>
                <th className="px-5 py-3 text-center">Tip</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((a) => (
                <tr key={a.id} className="border-t border-[#131c30] hover:bg-[#172239] cursor-pointer">
                  <td className="px-5 py-2.5 text-sm font-semibold">
                    <Link to={`/trading/${a.symbol}`} className="text-white hover:text-[#4fc3f7]">
                      {a.symbol}
                    </Link>
                  </td>
                  <td className="px-5 py-2.5 text-sm font-semibold text-white">{a.symbol}</td>
                  <td className="px-5 py-2.5 text-sm text-[#8b9bb4] truncate max-w-[300px]">{a.name}</td>
                  <td className="px-5 py-2.5 text-sm num text-right text-white">{fmt(a.currentPrice, 4)}</td>
                  <td className={`px-5 py-2.5 text-sm num text-right ${a.changePercent >= 0 ? 'text-[#4ade80]' : 'text-[#f87171]'}`}>
                    {a.changePercent >= 0 ? '+' : ''}{fmt(a.changePercent)}%
                  </td>
                  <td className="px-5 py-2.5 text-sm num text-right text-[#8b9bb4]">{fmt(a.dailyVolume, 0)}</td>
                  <td className="px-5 py-2.5 text-center">
                    <span className="text-xs px-2 py-0.5 rounded bg-[#1f2a44] text-[#8b9bb4]">{a.assetType}</span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}