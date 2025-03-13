import { Routes } from "@angular/router";

export const routes: Routes = [
  {
    path: "news",
    loadComponent: () =>
      import("./pages/news/news.component").then((mod) => mod.NewsComponent),
  },
  {
    path: "**",
    redirectTo: "news",
  },
];
