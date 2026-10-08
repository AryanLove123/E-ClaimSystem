export interface ApiResponse<T> {
  data: T;
  message: string;
  success: boolean;
  errors?: string[];
  traceId?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageSize: number;
  pageNumber: number;
  totalPages: number;
}
