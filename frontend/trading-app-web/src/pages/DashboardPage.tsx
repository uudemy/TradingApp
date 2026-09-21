import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuthStore } from '../store/auth';
import { api } from '../lib/api';
import type { PortfolioSummaryDto, AssetDto } from '../types/api';

const fmt = (n: number, digits = 2) =>
  n.toLocaleString('tr-TR', { minimumFractionDigits: digits, maximumFractionDigits: digits });

export default function DashboardPage() {
  const token = useAuthStore((s) => s.accessToken);
  const [portfolio, setPortfolio] = useState<PortfolioSummaryDto | null>(null);
  const [assets, setAssets] = useState<AssetDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!token) return;
    Promise.all([
      api.get<PortfolioSummaryDto>('/api/portfolio', token),
      api.get<AssetDto[]>('/api/assets', token),
    ])
      .then(([p, a]) => {
        setPortfolio(p);
        setAssets(a || []);
      })
      .finally(() => setLoading(false));
  }, [token]);

  if (loading) return <div className="p-8 text-[#8b9bb4]">Yükleniyor...</div>;

  const gainers = [...assets].sort((a, b) => b.changePercent - a.changePercent).slice(0, 5);
  const losers  = [...assets].sort((a, b) => a.changePercent - b.changePercent).slice(0, 5);

  return (
    <div className="p-8 space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-white">Dashboard</h1>
        <p className="text-sm text-[#8b9bb4] mt-1">Portföy özeti ve piyasa nabzı</p>
      </div>

      {/* Toplam kartları */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card title="Portföy Değeri" value={`${fmt(portfolio?.totalMarketValue ?? 0)} ₺`} />
        <Card
          title="Toplam K/Z"
          value={`${fmt(portfolio?.totalUnrealizedPnl ?? 0)} ₺`}
          sub={`${fmt(portfolio?.totalUnrealizedPnlPercent ?? 0)}%`}
          positive={(portfolio?.totalUnrealizedPnl ?? 0) >= 0}
        />
        <Card
          title="Günlük K/Z"
          value={`${fmt(portfolio?.totalDailyPnl ?? 0)} ₺`}
          positive={(portfolio?.totalDailyPnl ?? 0) >= 0}
        />
        <Card
          title="Toplam Pozisyon"
          value={`${portfolio?.positions.length ?? 0}`}
        />
      </div>

      {/* Cash balances */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {portfolio?.cashBalances.map((c) => (
          <div key={c.currency} className="bg-[#111a2e] border border-[#1f2a44] rounded-lg p-4">
            <div className="text-xs text-[#8b9bb4]">{c.currency} Bakiye</div>
            <div className="text-lg font-bold text-white num mt-1">{fmt(c.available)}</div>
            {c.locked > 0 && (
              <div className="text-xs text-[#fbbf24] mt-1">Kilitli: {fmt(c.locked)}</div>
            )}
          </div>
        ))}
      </div>

      {/* Gainers / Losers */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <TopList title="En Çok Yükselenler" items={gainers} />
        <TopList title="En Çok Düşenler" items={losers} />
      </div>
    </div>
  );
}

function Card({ title, value, sub, positive }: { title: string; value: string; sub?: string; positive?: boolean }) {
  const color = positive === undefined ? 'text-white' : positive ? 'text-[#4ade80]' : 'text-[#f87171]';
  return (
    <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg p-5">
      <div className="text-xs text-[#8b9bb4]">{title}</div>
      <div className={`text-xl font-bold mt-1 num ${color}`}>{value}</div>
      {sub && <div className={`text-xs mt-1 num ${color}`}>{sub}</div>}
    </div>
  );
}

function TopList({ title, items }: { title: string; items: AssetDto[] }) {
  return (
    <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg overflow-hidden">
      <div className="px-5 py-3 border-b border-[#1f2a44] text-sm font-semibold text-white">{title}</div>
      <table className="w-full">
        <tbody>
          {items.map((a) => (
            <tr key={a.id} className="border-b border-[#131c30] last:border-0 hover:bg-[#172239]">
              <td className="px-5 py-2 text-sm font-medium">
                <Link to={`/trading/${a.symbol}`} className="text-white hover:text-[#4fc3f7]">
                  {a.symbol}
                </Link>
              </td>
              <td className="px-5 py-2 text-sm font-medium text-white">{a.symbol}</td>
              <td className="px-5 py-2 text-sm text-[#8b9bb4] truncate max-w-[200px]">{a.name}</td>
              <td className="px-5 py-2 text-sm num text-right text-white">{fmt(a.currentPrice)}</td>
              <td className={`px-5 py-2 text-sm num text-right ${a.changePercent >= 0 ? 'text-[#4ade80]' : 'text-[#f87171]'}`}>
                {a.changePercent >= 0 ? '+' : ''}{fmt(a.changePercent)}%
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}