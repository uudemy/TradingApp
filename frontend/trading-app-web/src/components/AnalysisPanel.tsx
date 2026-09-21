interface Signal {
  name: string;
  value: number;
  interpretation: string;
  score: number;
  weight: number;
}

interface Props {
  score: number;
  category: string;
  rsi14: number | null;
  macdHistogram: number | null;
  ma20: number | null;
  ma50: number | null;
  volumeRatio: number | null;
  signals: Signal[];
}

const scoreColor = (s: number) =>
  s >= 80 ? 'text-[#4ade80]'
  : s >= 60 ? 'text-[#86efac]'
  : s >= 40 ? 'text-[#fbbf24]'
  : s >= 20 ? 'text-[#fb923c]'
  : 'text-[#f87171]';

export default function AnalysisPanel({
  score, category, rsi14, macdHistogram, ma20, ma50, volumeRatio, signals,
}: Props) {
  return (
    <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg p-5 space-y-4">
      <div>
        <div className="text-xs text-[#8b9bb4]">Tavan Potansiyeli Skoru</div>
        <div className={`text-4xl font-bold mt-1 num ${scoreColor(score)}`}>{score}</div>
        <div className={`text-sm mt-0.5 ${scoreColor(score)}`}>{category}</div>
      </div>

      {/* Skor bar */}
      <div className="h-2 bg-[#0b1220] rounded-full overflow-hidden">
        <div
          className={`h-full transition-all ${
            score >= 60 ? 'bg-[#4ade80]' : score >= 40 ? 'bg-[#fbbf24]' : 'bg-[#f87171]'
          }`}
          style={{ width: `${score}%` }}
        />
      </div>

      {/* İndikatör değerleri */}
      <div className="grid grid-cols-2 gap-3 text-xs">
        <Metric label="RSI(14)" value={rsi14} />
        <Metric label="MACD Hist" value={macdHistogram} />
        <Metric label="MA20" value={ma20} />
        <Metric label="MA50" value={ma50} />
        <Metric label="Hacim Oranı" value={volumeRatio} />
      </div>

      {/* Sinyaller */}
      <div>
        <div className="text-xs text-[#8b9bb4] mb-2">Sinyaller</div>
        <div className="space-y-1.5">
          {signals.map((s) => (
            <div key={s.name} className="flex items-center gap-2 text-xs">
              <span
                className={`w-6 text-center rounded py-0.5 font-mono font-bold ${
                  s.score > 0 ? 'bg-[#0d2f1f] text-[#4ade80]'
                  : s.score < 0 ? 'bg-[#2f0d0d] text-[#f87171]'
                  : 'bg-[#1f2a44] text-[#8b9bb4]'
                }`}
              >
                {s.score > 0 ? '+' : ''}{s.score}
              </span>
              <span className="text-white font-medium w-24">{s.name}</span>
              <span className="text-[#8b9bb4] flex-1 truncate">{s.interpretation}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

function Metric({ label, value }: { label: string; value: number | null }) {
  return (
    <div className="bg-[#0b1220] rounded px-2 py-1.5">
      <div className="text-[10px] text-[#8b9bb4]">{label}</div>
      <div className="text-white num">{value != null ? value.toFixed(2) : '—'}</div>
    </div>
  );
}