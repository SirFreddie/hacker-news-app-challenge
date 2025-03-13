import { Component, ViewChild, AfterViewInit, inject } from "@angular/core";
import { MatPaginator, MatPaginatorModule } from "@angular/material/paginator";
import { MatSort, MatSortModule, SortDirection } from "@angular/material/sort";
import { merge, of as observableOf } from "rxjs";
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  startWith,
  switchMap,
} from "rxjs/operators";
import { MatTableModule } from "@angular/material/table";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { ApiService } from "../../services/api.service";
import { New } from "../../interfaces/new.interface";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { FormControl, ReactiveFormsModule } from "@angular/forms";

@Component({
  selector: "app-news",
  imports: [
    MatProgressSpinnerModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
  ],
  templateUrl: "./news.component.html",
  styleUrl: "./news.component.scss",
})
export class NewsComponent {
  private _apiService = inject(ApiService);

  displayedColumns: string[] = ["title", "url"];
  data: New[] = [];
  filter = new FormControl("");

  resultsLength = 0;
  pageSize = 0;
  isLoadingResults = true;
  isRateLimitReached = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngAfterViewInit() {
    // If the user changes the sort order, reset back to the first page.
    this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 1));
    this.filter.valueChanges.subscribe(() => this.paginator.firstPage());

    merge(
      this.sort.sortChange,
      this.paginator.page,
      this.filter.valueChanges.pipe(debounceTime(500), distinctUntilChanged())
    )
      .pipe(
        startWith({}),
        switchMap(() => {
          this.isLoadingResults = true;
          return this._apiService!.getNews({
            page: this.paginator.pageIndex + 1,
            search: this.filter.value ?? "",
          }).pipe(catchError(() => observableOf(null)));
        }),
        map((response) => {
          // Flip flag to show that loading has finished.
          this.isLoadingResults = false;

          if (response === null) {
            return [];
          }

          this.resultsLength = response.totalCount;
          return response.data;
        })
      )
      .subscribe((news) => (this.data = news));
  }
}
