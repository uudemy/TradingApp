import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/auth';
import { ApiError } from '../lib/api';

export default function LoginPage() {
  const [email, setEmail] = useState('swagger@trading.local');
  const [password, setPassword] = useState('Passw0rd!');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const login = useAuthStore((s) => s.login);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await login(email, password);
      navigate('/dashboard');
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors.length > 0 ? err.errors.join(', ') : err.apiMessage);
      } else {
        setError('Giriş başarısız.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-[#0b1220] p-4">
      <div className="w-full max-w-md bg-[#111a2e] border border-[#1f2a44] rounded-xl p-8 shadow-2xl">
        <h1 className="text-2xl font-bold mb-1 bg-gradient-to-r from-[#4fc3f7] to-[#7c4dff] bg-clip-text text-transparent">
          TradingApp
        </h1>
        <p className="text-sm text-[#8b9bb4] mb-6">Hesabına giriş yap</p>

        {error && (
          <div className="mb-4 px-3 py-2 rounded-lg bg-[#2a1a1a] text-[#f87171] text-sm">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-xs text-[#8b9bb4] mb-1">E-posta</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-3 py-2 rounded-lg bg-[#0b1220] border border-[#1f2a44] text-white focus:border-[#4fc3f7] focus:outline-none"
            />
          </div>
          <div>
            <label className="block text-xs text-[#8b9bb4] mb-1">Şifre</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full px-3 py-2 rounded-lg bg-[#0b1220] border border-[#1f2a44] text-white focus:border-[#4fc3f7] focus:outline-none"
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-2.5 rounded-lg bg-gradient-to-r from-[#4fc3f7] to-[#7c4dff] text-white font-semibold disabled:opacity-50"
          >
            {loading ? 'Giriş yapılıyor...' : 'Giriş Yap'}
          </button>
        </form>

        <div className="mt-6 text-sm text-center text-[#8b9bb4]">
          Hesabın yok mu?{' '}
          <Link to="/register" className="text-[#4fc3f7] hover:underline">
            Kayıt Ol
          </Link>
        </div>
      </div>
    </div>
  );
}