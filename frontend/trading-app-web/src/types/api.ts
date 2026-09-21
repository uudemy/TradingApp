// Backend ApiResponse<T> zarfı
export interface ApiResponse<T> {
  success: boolean;
  message: string | null;
  data: T | null;
  errors: string[];
}

// Auth
export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isEmailVerified: boolean;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
  user: UserDto;
}

// Asset
export interface AssetDto {
  id: string;
  symbol: string;
  name: string;
  assetType: string;
  currency: string;
  currentPrice: number;
  previousClose: number;
  dailyVolume: number;
  isActive: boolean;
  changePercent: number;
}

// Market
export interface MarketPriceDto {
  symbol: string;
  price: number;
  previousClose: number;
  changePercent: number;
  dailyVolume: number;
  timestampUtc: string;
}

export interface CandleDto {
  openTime: string;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
}

// Portfolio
export interface PositionDto {
  id: string;
  assetId: string;
  symbol: string;
  name: string;
  currency: string;
  quantity: number;
  lockedQuantity: number;
  availableQuantity: number;
  averageCost: number;
  currentPrice: number;
  previousClose: number;
  marketValue: number;
  costBasis: number;
  unrealizedPnl: number;
  unrealizedPnlPercent: number;
  dailyPnl: number;
  realizedPnl: number;
  purchaseDate: string | null;
  notes: string | null;
}

export interface CashBalanceDto {
  currency: string;
  available: number;
  locked: number;
  total: number;
}

export interface PortfolioSummaryDto {
  totalMarketValue: number;
  totalCostBasis: number;
  totalUnrealizedPnl: number;
  totalUnrealizedPnlPercent: number;
  totalDailyPnl: number;
  cashBalances: CashBalanceDto[];
  positions: PositionDto[];
}

// SignalR market price update
export interface MarketPriceUpdate {
  symbol: string;
  price: number;
  changePercent: number;
  dailyVolume: number;
  timestampUtc: string;
}
// Watchlist
export interface WatchlistItemDto {
  assetId: string;
  symbol: string;
  name: string;
  assetType: string;
  currency: string;
  currentPrice: number;
  changePercent: number;
  addedAt: string;
}

// Price Alert
export interface PriceAlertDto {
  id: string;
  assetId: string;
  symbol: string;
  condition: string;
  targetPrice: number;
  isActive: boolean;
  triggeredAt: string | null;
  createdAt: string;
}

// Notification
export interface NotificationDto {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationPayload {
  id: string;
  title: string;
  message: string;
  type: string;
  createdAt: string;
}


export interface IndicatorPointDto {
  time: string;
  value: number | null;
}

export interface MacdPointDto {
  time: string;
  macd: number | null;
  signal: number | null;
  histogram: number | null;
}

export interface IndicatorSeriesDto {
  ma20: IndicatorPointDto[];
  ma50: IndicatorPointDto[];
  ma200: IndicatorPointDto[];
  bollingerUpper: IndicatorPointDto[];
  bollingerMiddle: IndicatorPointDto[];
  bollingerLower: IndicatorPointDto[];
  rsi: IndicatorPointDto[];
  macd: MacdPointDto[];
}

export interface AnalysisScanItemDto {
  assetId: string;
  symbol: string;
  name: string;
  assetType: string;
  currentPrice: number;
  changePercent: number;
  rsi14: number | null;
  volumeRatio: number | null;
  ceilingScore: number;
  ceilingCategory: string;
}

export interface ScreenerResultDto {
  totalScanned: number;
  totalMatched: number;
  interval: string;
  items: AnalysisScanItemDto[];
  scannedAt: string;
}