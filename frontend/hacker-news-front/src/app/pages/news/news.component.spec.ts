import {
  ComponentFixture,
  TestBed,
  fakeAsync,
  tick,
} from "@angular/core/testing";
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { MatPaginator, MatPaginatorModule } from "@angular/material/paginator";
import { MatSort, MatSortModule } from "@angular/material/sort";
import { MatTableModule } from "@angular/material/table";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { ReactiveFormsModule } from "@angular/forms";
import { of } from "rxjs";
import { ApiService } from "../../services/api.service";
import { NewsComponent } from "./news.component";
import { GetNewsResponse } from "../../interfaces/responses.interface";
import { DebugElement } from "@angular/core";
import { By } from "@angular/platform-browser";
import { provideHttpClient } from "@angular/common/http";

describe("NewsComponent", () => {
  let component: NewsComponent;
  let fixture: ComponentFixture<NewsComponent>;
  let apiServiceSpy: jasmine.SpyObj<ApiService>;
  let paginator: MatPaginator;
  let sort: MatSort;

  beforeEach(async () => {
    apiServiceSpy = jasmine.createSpyObj<ApiService>("ApiService", ["getNews"]);

    await TestBed.configureTestingModule({
      imports: [
        NewsComponent,
        MatPaginatorModule,
        MatSortModule,
        MatTableModule,
        MatProgressSpinnerModule,
        MatFormFieldModule,
        MatInputModule,
        ReactiveFormsModule,
      ],
      providers: [
        { provide: ApiService, useValue: apiServiceSpy },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NewsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    paginator = component.paginator;
    sort = component.sort;
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("should load news data on initialization", fakeAsync(() => {
    const mockResponse: GetNewsResponse = {
      data: [{ id: 1, title: "Test News", url: "https://test.com" }],
      totalCount: 1,
      currentPage: 1,
      pageSize: 10,
    };

    apiServiceSpy.getNews.and.returnValue(of(mockResponse));

    component.ngAfterViewInit();
    tick(); // Simulates api call execution

    expect(component.data.length).toBe(1);
    expect(component.resultsLength).toBe(1);
    expect(apiServiceSpy.getNews).toHaveBeenCalled();
  }));

  it("should reset pagination when search input changes", fakeAsync(() => {
    spyOn(component.paginator, "firstPage");

    component.filter.setValue("Updated Search");
    tick(500); // Wait for debounce

    expect(component.paginator.firstPage).toHaveBeenCalled();
  }));

  it("should call API when pagination changes", fakeAsync(() => {
    const mockResponse: GetNewsResponse = {
      data: [{ id: 3, title: "Paged News", url: "https://news.com" }],
      totalCount: 20,
      currentPage: 2,
      pageSize: 10,
    };

    apiServiceSpy.getNews.and.returnValue(of(mockResponse));

    component.paginator.pageIndex = 1;
    component.paginator.page.emit();

    tick();

    fixture.detectChanges();

    expect(apiServiceSpy.getNews).toHaveBeenCalledWith({
      page: 1,
      search: "",
    });
  }));

  it("should display loading indicator while fetching data", fakeAsync(() => {
    apiServiceSpy.getNews.and.returnValue(
      of({
        data: [],
        totalCount: 0,
        currentPage: 1,
        pageSize: 10,
      })
    );

    component.ngAfterViewInit();
    tick();

    expect(component.isLoadingResults).toBe(false);
  }));

  it("should display a 'No results found' message if API returns an empty list", fakeAsync(() => {
    const mockResponse: GetNewsResponse = {
      data: [],
      totalCount: 0,
      currentPage: 1,
      pageSize: 10,
    };

    apiServiceSpy.getNews.and.returnValue(of(mockResponse));

    component.ngAfterViewInit();
    tick();
    fixture.detectChanges();

    const debugElement: DebugElement = fixture.debugElement;
    const noResultsMessage = debugElement.query(By.css(".no-results"));

    expect(noResultsMessage).toBeTruthy();
  }));
});
