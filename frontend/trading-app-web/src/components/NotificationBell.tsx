import { useCallback, useEffect, useRef, useState } from 'react';
import { useAuthStore } from '../store/auth';
import { api } from '../lib/api';
import type { NotificationDto, NotificationPayload } from '../types/api';
import { useNotificationHub } from '../hooks/useNotificationHub';

export default function NotificationBell() {
  const token = useAuthStore((s) => s.accessToken);
  const [open, setOpen] = useState(false);
  const [items, setItems] = useState<NotificationDto[]>([]);
  const [toast, setToast] = useState<NotificationPayload | null>(null);
  const ref = useRef<HTMLDivElement>(null);

  const unread = items.filter((n) => !n.isRead).length;

  const load = useCallback(async () => {
    if (!token) return;
    const data = await api.get<NotificationDto[]>('/api/notifications', token);
    setItems(data || []);
  }, [token]);

  useEffect(() => { load(); }, [load]);

  // SignalR canlı bildirim
  useNotificationHub((n) => {
    setToast(n);
    setTimeout(() => setToast(null), 6000);
    setItems((prev) => [
      {
        id: n.id,
        title: n.title,
        message: n.message,
        type: n.type,
        isRead: false,
        createdAt: n.createdAt,
      },
      ...prev,
    ]);
  });

  // Dış tıklama
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const markRead = async (id: string) => {
    await api.put(`/api/notifications/${id}/read`, undefined, token);
    setItems((prev) => prev.map((n) => (n.id === id ? { ...n, isRead: true } : n)));
  };

  return (
    <>
      <div ref={ref} className="relative">
        <button
          onClick={() => setOpen(!open)}
          className="relative w-10 h-10 rounded-lg bg-[#111a2e] border border-[#1f2a44] hover:bg-[#172239] flex items-center justify-center text-lg"
        >
          🔔
          {unread > 0 && (
            <span className="absolute -top-1 -right-1 min-w-[18px] h-[18px] px-1 rounded-full bg-[#f87171] text-white text-[10px] flex items-center justify-center font-bold">
              {unread > 9 ? '9+' : unread}
            </span>
          )}
        </button>

        {open && (
          <div className="absolute right-0 top-12 w-96 bg-[#111a2e] border border-[#1f2a44] rounded-lg shadow-2xl overflow-hidden z-50">
            <div className="px-4 py-3 border-b border-[#1f2a44] flex items-center justify-between">
              <span className="text-sm font-semibold text-white">Bildirimler</span>
              <span className="text-xs text-[#8b9bb4]">{items.length} toplam</span>
            </div>

            <div className="max-h-96 overflow-y-auto">
              {items.length === 0 ? (
                <div className="p-8 text-center text-sm text-[#8b9bb4]">Henüz bildirim yok</div>
              ) : (
                items.slice(0, 20).map((n) => (
                  <div
                    key={n.id}
                    className={`px-4 py-3 border-b border-[#131c30] hover:bg-[#172239] cursor-pointer ${
                      !n.isRead ? 'bg-[#0f1a30]' : ''
                    }`}
                    onClick={() => !n.isRead && markRead(n.id)}
                  >
                    <div className="flex items-start gap-2">
                      {!n.isRead && <span className="w-2 h-2 rounded-full bg-[#4fc3f7] mt-1.5 flex-shrink-0" />}
                      <div className="flex-1 min-w-0">
                        <div className="text-sm font-medium text-white truncate">{n.title}</div>
                        <div className="text-xs text-[#8b9bb4] mt-0.5 line-clamp-2">{n.message}</div>
                        <div className="text-[10px] text-[#5f6f88] mt-1">
                          {new Date(n.createdAt).toLocaleString('tr-TR')}
                        </div>
                      </div>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        )}
      </div>

      {/* Toast */}
      {toast && (
        <div className="fixed bottom-6 right-6 w-80 bg-[#111a2e] border border-[#4fc3f7] rounded-lg shadow-2xl p-4 z-50 animate-pulse">
          <div className="flex items-start gap-2">
            <span className="text-xl">🔔</span>
            <div className="flex-1">
              <div className="text-sm font-semibold text-white">{toast.title}</div>
              <div className="text-xs text-[#8b9bb4] mt-1">{toast.message}</div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}