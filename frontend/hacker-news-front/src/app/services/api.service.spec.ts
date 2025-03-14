import { TestBed } from "@angular/core/testing";
import {
  HttpTestingController,
  provideHttpClientTesting,
} from "@angular/common/http/testing";
import { ApiService } from "./api.service";
import { GetNewsResponse } from "../interfaces/responses.interface";
import { provideHttpClient } from "@angular/common/http";

describe("ApiService", () => {
  let service: ApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [],
      providers: [ApiService, provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it("should be created", () => {
    expect(service).toBeTruthy();
  });

  it("should fetch news with default parameters", () => {
    const mockResponse: GetNewsResponse = {
      data: [{ id: 1, title: "Test News", url: "https://test.com" }],
      totalCount: 1,
      currentPage: 1,
      pageSize: 10,
    };

    service.getNews({}).subscribe((response) => {
      expect(response).toEqual(mockResponse);
    });

    const req = httpMock.expectOne("http://localhost:5149/api/news");
    expect(req.request.method).toBe("GET");
    req.flush(mockResponse);
  });

  it("should fetch news with search parameter", () => {
    const mockResponse: GetNewsResponse = {
      data: [{ id: 2, title: "Google News", url: "https://google.com" }],
      totalCount: 1,
      currentPage: 1,
      pageSize: 10,
    };

    service.getNews({ search: "Google" }).subscribe((response) => {
      expect(response).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(
      "http://localhost:5149/api/news?search=Google"
    );
    expect(req.request.method).toBe("GET");
    req.flush(mockResponse);
  });

  it("should fetch news with pagination parameters", () => {
    const mockResponse: GetNewsResponse = {
      data: [{ id: 3, title: "Paged News", url: "https://test.com" }],
      totalCount: 20,
      currentPage: 2,
      pageSize: 5,
    };

    service.getNews({ page: 2, pageSize: 5 }).subscribe((response) => {
      expect(response).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(
      "http://localhost:5149/api/news?page=2&pageSize=5"
    );
    expect(req.request.method).toBe("GET");
    req.flush(mockResponse);
  });

  it("should handle HTTP errors gracefully", () => {
    service.getNews({}).subscribe({
      next: () => fail("Should have failed with 500 status"),
      error: (error) => {
        expect(error.status).toBe(500);
      },
    });

    const req = httpMock.expectOne("http://localhost:5149/api/news");
    expect(req.request.method).toBe("GET");
    req.flush("Server error", {
      status: 500,
      statusText: "Internal Server Error",
    });
  });
});
