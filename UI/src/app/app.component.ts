import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from './../environments/environment';
import { ImageDto } from './models';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.sass'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit {
  readonly BaseURL: string = window.Api_URL;
  readonly ENTER_KEY = 'Enter';
  Images: ImageDto[];
  keyword: string;
  isLoading = false;
  @ViewChild('search') searchInput: ElementRef;

  constructor(private changeRef: ChangeDetectorRef, private httpclient: HttpClient, private route: ActivatedRoute, private router: Router) {
    this.BaseURL = environment.Api_URL;
    this.BaseURL = this.BaseURL.endsWith('/') ? this.BaseURL : this.BaseURL + '/';

  }

  ngOnInit(): void {
    this.keyword = localStorage.getItem('keyword');
    this.Images = JSON.parse(localStorage.getItem('lastdata'));

    this.route.queryParams.subscribe(params => {
      const value = params.keyword;
      this.keyword = value;
      const isValid = this.IsValidKeyword(value);
      if (!isValid) {
        this.ClearResults();
        return;
      }
      this.searchInput.nativeElement.value = value;
      this.SearchApi(value);
    })
  }

  private IsValidKeyword(value: string): boolean {
    return value && value.trim().length > 2;
  }

  private ClearResults(): void {
    this.Images = [];
    this.changeRef.detectChanges();
  }

  Find(value: string): void {
    const isValid = this.IsValidKeyword(value);
    if (!isValid) {
      this.ClearResults();
      return;
    }
    this.keyword = value;
    this.router.navigate([], { queryParams: { keyword: value } });

  }

  get searchUrl() {
    return this.BaseURL + 'v1/AirTeam/Search';
  }

  SearchApi(value: string) {
    this.isLoading = true;
    this.httpclient.get<ImageDto[]>(this.searchUrl + '?keyword=' + value)
      .subscribe(data => {
        this.Images = data;
        localStorage.setItem('keyword', this.keyword);
        localStorage.setItem('lastdata', JSON.stringify(this.Images));
        
        this.changeRef.detectChanges();
        
      }, error => {
        this.Images = [];
      }, () => {
        this.isLoading = false;
        this.changeRef.detectChanges();
      });
  }

  onKeyUp(event: KeyboardEvent) {
    if (event.key === this.ENTER_KEY) {
      this.Find((event.target as HTMLInputElement).value);
    }
  }
}
