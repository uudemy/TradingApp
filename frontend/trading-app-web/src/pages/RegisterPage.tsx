import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/auth';
import { ApiError } from '../lib/api';

export default function RegisterPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const register = useAuthStore((s) => s.register);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await register(email, password, firstName, lastName);
      navigate('/dashboard');
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors.length > 0 ? err.errors.join(', ') : err.apiMessage);
      } else {
        setError('Kayıt başarısız.');
      }
    } finally {
      setLoading(false);
    }
  };

  const inputClass = "w-full px-3 py-2 rounded-lg bg-[#0b1220] border border-[#1f2a44] text-white focus:border-[#4fc3f7] focus:outline-none";

  return (
    <div className="min-h-screen flex items-center justify-center bg-[#0b1220] p-4">
      <div className="w-full max-w-md bg-[#111a2e] border border-[#1f2a44] rounded-xl p-8 shadow-2xl">
        <h1 className="text-2xl font-bold mb-1 bg-gradient-to-r from-[#4fc3f7] to-[#7c4dff] bg-clip-text text-transparent">
          Kayıt Ol
        </h1>
        <p className="text-sm text-[#8b9bb4] mb-6">Demo hesabına 100.000 TRY + 10.000 USD + 10.000 USDT tanımlanır.</p>

        {error && (
          <div className="mb-4 px-3 py-2 rounded-lg bg-[#2a1a1a] text-[#f87171] text-sm">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs text-[#8b9bb4] mb-1">Ad</label>
              <input value={firstName} onChange={(e) => setFirstName(e.target.value)} required className={inputClass} />
            </div>
            <div>
              <label className="block text-xs text-[#8b9bb4] mb-1">Soyad</label>
              <input value={lastName} onChange={(e) => setLastName(e.target.value)} required className={inputClass} />
            </div>
          </div>
          <div>
            <label className="block text-xs text-[#8b9bb4] mb-1">E-posta</label>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required className={inputClass} />
          </div>
          <div>
            <label className="block text-xs text-[#8b9bb4] mb-1">Şifre (min 8 karakter)</label>
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={8} className={inputClass} />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-2.5 rounded-lg bg-gradient-to-r from-[#4fc3f7] to-[#7c4dff] text-white font-semibold disabled:opacity-50"
          >
            {loading ? 'Kayıt yapılıyor...' : 'Kayıt Ol'}
          </button>
        </form>

        <div className="mt-6 text-sm text-center text-[#8b9bb4]">
          Hesabın var mı?{' '}
          <Link to="/login" className="text-[#4fc3f7] hover:underline">
            Giriş Yap
          </Link>
        </div>
      </div>
    </div>
  );
}