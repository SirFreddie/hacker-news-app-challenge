import { HttpClient, HttpParams } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { New } from "../interfaces/new.interface";
import { GetNewsResponse } from "../interfaces/responses.interface";

@Injectable({
  providedIn: "root",
})
export class ApiService {
  private _httpClient = inject(HttpClient);

  private _baseUrl = "http://localhost:5149/api";

  constructor() {}

  getNews(
    filter: { page: number; search?: string } = { page: 1 }
  ): Observable<GetNewsResponse> {
    let httpParams = new HttpParams();

    if (filter.search) {
      httpParams = httpParams.set("search", filter.search);
    }

    if (filter.page) {
      httpParams = httpParams.set("page", filter.page.toString());
    }

    return this._httpClient.get<GetNewsResponse>(`${this._baseUrl}/news`, {
      params: httpParams,
    });
  }
}
