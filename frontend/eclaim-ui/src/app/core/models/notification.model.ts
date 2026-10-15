export interface AppNotification {
  id: number;
  type: string;
  subject: string;
  message: string;
  isRead: boolean;
  claimId?: number;
  createdAt: string;
}

export interface NotificationSummary {
  unreadCount: number;
  items: AppNotification[];
}