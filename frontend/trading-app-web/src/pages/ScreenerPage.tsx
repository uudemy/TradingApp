import { useCallback, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useAuthStore } from "../store/auth";
import { api } from "../lib/api";
import { useScreenerStore } from '../store/screener';
import type { ScreenerResultDto, AnalysisScanItemDto } from "../types/api";

const fmt = (n: number | null | undefined, d = 2) =>
  n == null
    ? "—"
    : n.toLocaleString("tr-TR", {
        minimumFractionDigits: d,
        maximumFractionDigits: d,
      });

const scoreColor = (s: number) =>
  s >= 80
    ? "text-[#4ade80]"
    : s >= 60
      ? "text-[#86efac]"
      : s >= 40
        ? "text-[#fbbf24]"
        : s >= 20
          ? "text-[#fb923c]"
          : "text-[#f87171]";

const scoreBg = (s: number) =>
  s >= 80
    ? "bg-[#0d2f1f] border-[#4ade80]"
    : s >= 60
      ? "bg-[#0d2a1a] border-[#86efac]"
      : s >= 40
        ? "bg-[#2a2010] border-[#fbbf24]"
        : s >= 20
          ? "bg-[#2a180a] border-[#fb923c]"
          : "bg-[#2a0d0d] border-[#f87171]";

const signal = (s: number) =>
  s >= 80
    ? { label: "GÜÇLÜ AL", color: "text-[#4ade80] bg-[#0d2f1f]" }
    : s >= 60
      ? { label: "AL", color: "text-[#86efac] bg-[#0d2a1a]" }
      : s >= 40
        ? { label: "TUT", color: "text-[#fbbf24] bg-[#2a2010]" }
        : s >= 20
          ? { label: "SAT", color: "text-[#fb923c] bg-[#2a180a]" }
          : { label: "GÜÇLÜ SAT", color: "text-[#f87171] bg-[#2a0d0d]" };

export default function ScreenerPage() {
  const token = useAuthStore((s) => s.accessToken);
  const lastResult = useScreenerStore((s) => s.lastResult);
  const setScreenerResult = useScreenerStore((s) => s.setResult);
  const [data, setData] = useState<ScreenerResultDto | null>(lastResult);
  const [loading, setLoading] = useState(false);
  const [progress, setProgress] = useState<string>("");

  // Filtreler
  const [assetType, setAssetType] = useState<string>("Stock");
  const [interval, setIntervalPeriod] = useState<string>("1D");
  const [minScore, setMinScore] = useState<number>(50);
  const [maxRsi, setMaxRsi] = useState<number | "">("");
  const [category, setCategory] = useState<string>("");

  // Tablo sıralama
  const [sortKey, setSortKey] = useState<"score" | "change" | "rsi" | "volume">(
    "score",
  );
  const [sortDir, setSortDir] = useState<"asc" | "desc">("desc");

  const scan = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setProgress("Hisseler taranıyor, 1-2 dakika sürebilir...");
    try {
      const params = new URLSearchParams();
      if (assetType) params.set("assetType", assetType);
      if (minScore > 0) params.set("minScore", String(minScore));
      if (maxRsi !== "") params.set("maxRsi", String(maxRsi));
      if (category) params.set("category", category);
      params.set("interval", interval);
      params.set("limit", "500");

      const res = await api.get<ScreenerResultDto>(
        `/api/analysis/screener?${params}`,
        token,
      );
      if (res) {
        setData(res);
        setScreenerResult(res, { assetType, interval, minScore, maxRsi, category });
      }
    } finally {
      setLoading(false);
      setProgress("");
    }
  }, [token, assetType, interval, minScore, maxRsi, category]);

  // İlk yükleme
  useEffect(() => {
    scan();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const sorted = useMemo(() => {
    if (!data) return [];
    const dir = sortDir === "asc" ? 1 : -1;
    return [...data.items].sort((a, b) => {
      switch (sortKey) {
        case "score":
          return dir * (a.ceilingScore - b.ceilingScore);
        case "change":
          return dir * (a.changePercent - b.changePercent);
        case "rsi":
          return dir * ((a.rsi14 ?? 50) - (b.rsi14 ?? 50));
        case "volume":
          return dir * ((a.volumeRatio ?? 1) - (b.volumeRatio ?? 1));
      }
    });
  }, [data, sortKey, sortDir]);

  const toggleSort = (key: typeof sortKey) => {
    if (sortKey === key) setSortDir(sortDir === "asc" ? "desc" : "asc");
    else {
      setSortKey(key);
      setSortDir("desc");
    }
  };

  const arrow = (key: typeof sortKey) =>
    sortKey === key ? (sortDir === "asc" ? " ↑" : " ↓") : "";

  const inputClass =
    "px-3 py-2 rounded-lg bg-[#0b1220] border border-[#1f2a44] text-white focus:border-[#4fc3f7] focus:outline-none text-sm";

  return (
    <div className="p-8 space-y-6">
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">Fırsat Tarayıcı</h1>
          <p className="text-sm text-[#8b9bb4] mt-1">
            Tüm hisseleri teknik analizle tarar, alım sinyali verenleri sıralar
          </p>
        </div>
        <button
          onClick={scan}
          disabled={loading}
          className="px-5 py-2 rounded-lg bg-gradient-to-r from-[#4fc3f7] to-[#7c4dff] text-white text-sm font-semibold disabled:opacity-50"
        >
          {loading ? "Taranıyor..." : "🔄 Yenile"}
        </button>
      </div>

      {/* Filtreler */}
      <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg p-4 flex flex-wrap gap-3 items-end">
        <div>
          <label className="block text-xs text-[#8b9bb4] mb-1">Tip</label>
          <select
            value={assetType}
            onChange={(e) => setAssetType(e.target.value)}
            className={`${inputClass} w-40`}
          >
            <option value="Stock">Hisse</option>
            <option value="Crypto">Kripto</option>
            <option value="">Tümü</option>
          </select>
        </div>

        <div>
          <label className="block text-xs text-[#8b9bb4] mb-1">Min Skor</label>
          <input
            type="number"
            min={0}
            max={100}
            value={minScore}
            onChange={(e) => setMinScore(parseInt(e.target.value) || 0)}
            className={`${inputClass} w-24`}
          />
        </div>

        <div>
          <label className="block text-xs text-[#8b9bb4] mb-1">Maks RSI</label>
          <input
            type="number"
            min={0}
            max={100}
            placeholder="örn. 70"
            value={maxRsi}
            onChange={(e) =>
              setMaxRsi(e.target.value === "" ? "" : parseInt(e.target.value))
            }
            className={`${inputClass} w-24`}
          />
          <div className="text-[10px] text-[#5f6f88] mt-1">
            Aşırı alımı filtrele
          </div>
        </div>

        <div>
          <label className="block text-xs text-[#8b9bb4] mb-1">Kategori</label>
          <select
            value={category}
            onChange={(e) => setCategory(e.target.value)}
            className={`${inputClass} w-40`}
          >
            <option value="">Tümü</option>
            <option value="Güçlü Yükseliş">Güçlü Yükseliş</option>
            <option value="Yükseliş">Yükseliş</option>
            <option value="Nötr">Nötr</option>
          </select>
        </div>
        <div>
          <label className="block text-xs text-[#8b9bb4] mb-1">Periyot</label>
          <div className="flex gap-1 bg-[#0b1220] border border-[#1f2a44] rounded-lg p-1">
            {[
              { key: "1D", label: "Günlük" },
              { key: "1W", label: "Haftalık" },
              { key: "1MO", label: "Aylık" },
            ].map((p) => (
              <button
                key={p.key}
                type="button"
                onClick={() => setIntervalPeriod(p.key)}
                className={`px-3 py-1.5 text-xs font-medium rounded-md transition ${
                  interval === p.key
                    ? "bg-[#1d2b48] text-white border border-[#4fc3f7]"
                    : "text-[#8b9bb4] hover:text-white border border-transparent"
                }`}
              >
                {p.label}
              </button>
            ))}
          </div>
        </div>

        <button
          onClick={scan}
          disabled={loading}
          className="px-4 py-2 rounded-lg bg-[#1d2b48] border border-[#4fc3f7] text-white text-sm font-semibold disabled:opacity-50"
        >
          Filtrele
        </button>
      </div>

      {/* Özet */}
      {data && (
        <div className="grid grid-cols-2 md:grid-cols-6 gap-3">
          <Stat
            label="Periyot"
            value={0}
            color="#4fc3f7"
            text={
              data.interval === "1D"
                ? "Günlük"
                : data.interval === "1W"
                  ? "Haftalık"
                  : "Aylık"
            }
          />
          <Stat label="Taranan" value={data.totalScanned} />
          <Stat label="Eşleşen" value={data.totalMatched} highlight />
          <Stat
            label="Güçlü Yükseliş"
            value={data.items.filter((x) => x.ceilingScore >= 80).length}
            color="#4ade80"
          />
          <Stat
            label="Yükseliş"
            value={
              data.items.filter(
                (x) => x.ceilingScore >= 60 && x.ceilingScore < 80,
              ).length
            }
            color="#86efac"
          />
          <Stat
            label="Nötr"
            value={
              data.items.filter(
                (x) => x.ceilingScore >= 40 && x.ceilingScore < 60,
              ).length
            }
            color="#fbbf24"
          />
        </div>
      )}

      {loading && (
        <div className="text-center py-16 text-[#8b9bb4]">
          <div className="inline-block animate-spin text-2xl mb-3">⟳</div>
          <div className="text-sm">{progress}</div>
        </div>
      )}

      {/* Tablo */}
      {!loading && data && data.items.length > 0 && (
        <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg overflow-hidden">
          <div className="px-5 py-3 border-b border-[#1f2a44] flex items-center justify-between text-xs text-[#8b9bb4]">
            <span>{sorted.length} sonuç gösteriliyor</span>
            <span>
              Son güncelleme:{" "}
              {new Date(data.scannedAt).toLocaleTimeString("tr-TR")}
            </span>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-[#0f1729] sticky top-0">
                <tr className="text-xs text-[#8b9bb4] uppercase">
                  <th className="px-4 py-3 text-left">#</th>
                  <th className="px-4 py-3 text-left">Sembol</th>
                  <th className="px-4 py-3 text-left">İsim</th>
                  <th className="px-4 py-3 text-right">Fiyat</th>
                  <th
                    onClick={() => toggleSort("change")}
                    className="px-4 py-3 text-right cursor-pointer hover:text-white"
                  >
                    Değişim{arrow("change")}
                  </th>
                  <th
                    onClick={() => toggleSort("rsi")}
                    className="px-4 py-3 text-right cursor-pointer hover:text-white"
                  >
                    RSI{arrow("rsi")}
                  </th>
                  <th
                    onClick={() => toggleSort("volume")}
                    className="px-4 py-3 text-right cursor-pointer hover:text-white"
                  >
                    Hacim Oranı{arrow("volume")}
                  </th>
                  <th
                    onClick={() => toggleSort("score")}
                    className="px-4 py-3 text-center cursor-pointer hover:text-white"
                  >
                    Skor{arrow("score")}
                  </th>
                  <th className="px-4 py-3 text-center">Sinyal</th>
                </tr>
              </thead>
              <tbody>
                {sorted.map((it, i) => (
                  <Row key={it.assetId} item={it} rank={i + 1} />
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {!loading && data && data.items.length === 0 && (
        <div className="text-center py-16 text-[#8b9bb4]">
          <div className="text-lg mb-2">Eşleşen sonuç yok</div>
          <div className="text-sm">
            Filtreleri gevşetmeyi dene (min skor düşür)
          </div>
        </div>
      )}
    </div>
  );
}

function Stat({
  label,
  value,
  highlight,
  color,
  text,
}: {
  label: string;
  value: number;
  highlight?: boolean;
  color?: string;
  text?: string;
}) {
  return (
    <div
      className={`bg-[#111a2e] border rounded-lg p-4 ${highlight ? "border-[#4fc3f7]" : "border-[#1f2a44]"}`}
    >
      <div className="text-xs text-[#8b9bb4]">{label}</div>
      <div
        className="text-2xl font-bold mt-1 num"
        style={{ color: color || (highlight ? "#4fc3f7" : "#fff") }}
      >
        {text ?? value}
      </div>
    </div>
  );
}

function Row({ item, rank }: { item: AnalysisScanItemDto; rank: number }) {
  const sig = signal(item.ceilingScore);
  return (
    <tr className="border-t border-[#131c30] hover:bg-[#172239]">
      <td className="px-4 py-3 text-xs text-[#5f6f88] num">{rank}</td>
      <td className="px-4 py-3 text-sm font-semibold">
        <Link
          to={`/trading/${item.symbol}`}
          className="text-white hover:text-[#4fc3f7]"
        >
          {item.symbol}
        </Link>
      </td>
      <td className="px-4 py-3 text-sm text-[#8b9bb4] truncate max-w-[280px]">
        {item.name}
      </td>
      <td className="px-4 py-3 text-sm num text-right text-white">
        {fmt(item.currentPrice, 4)}
      </td>
      <td
        className={`px-4 py-3 text-sm num text-right ${item.changePercent >= 0 ? "text-[#4ade80]" : "text-[#f87171]"}`}
      >
        {item.changePercent >= 0 ? "+" : ""}
        {fmt(item.changePercent)}%
      </td>
      <td
        className={`px-4 py-3 text-sm num text-right ${
          item.rsi14 == null
            ? "text-[#5f6f88]"
            : item.rsi14 < 30
              ? "text-[#4ade80]"
              : item.rsi14 > 70
                ? "text-[#f87171]"
                : "text-white"
        }`}
      >
        {item.rsi14 != null ? item.rsi14.toFixed(1) : "—"}
      </td>
      <td
        className={`px-4 py-3 text-sm num text-right ${
          item.volumeRatio != null && item.volumeRatio >= 1.5
            ? "text-[#4ade80]"
            : "text-[#8b9bb4]"
        }`}
      >
        {item.volumeRatio != null ? `${item.volumeRatio.toFixed(2)}x` : "—"}
      </td>
      <td className="px-4 py-3 text-center">
        <div
          className={`inline-flex items-center justify-center w-12 h-9 rounded-lg border ${scoreBg(item.ceilingScore)}`}
        >
          <span
            className={`text-base font-bold num ${scoreColor(item.ceilingScore)}`}
          >
            {item.ceilingScore}
          </span>
        </div>
      </td>
      <td className="px-4 py-3 text-center">
        <span
          className={`inline-block px-3 py-1 rounded-md text-xs font-bold ${sig.color}`}
        >
          {sig.label}
        </span>
      </td>
    </tr>
  );
}
