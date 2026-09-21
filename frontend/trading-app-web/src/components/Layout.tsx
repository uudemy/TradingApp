import { Outlet, NavLink } from 'react-router-dom';
import { useAuthStore } from '../store/auth';
import { useNavigate } from 'react-router-dom';
import NotificationBell from './NotificationBell';

const nav = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/markets',   label: 'Piyasalar' },
  { to: '/watchlist', label: 'İzleme Listesi' },
  { to: '/portfolio', label: 'Portföyüm' },
  { to: '/alerts',    label: 'Alarmlar' },
];

export default function Layout() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="flex min-h-screen bg-[#0b1220]">
      {/* Sidebar */}
      <aside className="w-60 bg-[#111a2e] border-r border-[#1f2a44] flex flex-col">
        <div className="p-5 border-b border-[#1f2a44]">
          <div className="text-lg font-bold bg-gradient-to-r from-[#4fc3f7] to-[#7c4dff] bg-clip-text text-transparent">
            TradingApp
          </div>
          <div className="text-xs text-[#8b9bb4] mt-0.5">Borsa Takip</div>
        </div>

        <nav className="flex-1 p-3 space-y-1">
          {nav.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `block px-3 py-2 rounded-lg text-sm transition ${
                  isActive
                    ? 'bg-[#1d2b48] text-white border-l-2 border-[#4fc3f7]'
                    : 'text-[#8b9bb4] hover:bg-[#172239] hover:text-white'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="p-3 border-t border-[#1f2a44]">
          <div className="text-xs text-[#8b9bb4] mb-2 px-2">
            <div className="truncate font-medium text-white">{user?.firstName} {user?.lastName}</div>
            <div className="truncate">{user?.email}</div>
          </div>
          <button
            onClick={handleLogout}
            className="w-full px-3 py-2 text-sm text-left rounded-lg text-[#f87171] hover:bg-[#2a1a1a]"
          >
            Çıkış Yap
          </button>
        </div>
      </aside>

      {/* Main */}
      <div className="flex-1 flex flex-col overflow-hidden">
        <header className="flex items-center justify-end gap-3 px-6 py-3 border-b border-[#1f2a44] bg-[#0b1220]">
          <NotificationBell />
        </header>
        <main className="flex-1 overflow-y-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}