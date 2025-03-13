import { New } from "./new.interface";

export interface GetNewsResponse {
  data: New[];
  currentPage: number;
  totalCount: number;
  pageSize: number;
}
