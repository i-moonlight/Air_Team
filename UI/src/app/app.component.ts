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
  readonly BaseURL: string;
  readonly ENTER_KEY = 'Enter';
  Images: ImageDto[];
  keyword: string;
  isLoading = false;
  noResultFound = false;
  @ViewChild('search') searchInput: ElementRef;

  constructor(private changeRef: ChangeDetectorRef, private httpclient: HttpClient, private route: ActivatedRoute, private router: Router) {
    this.BaseURL = environment.Api_URL;
    this.BaseURL = this.BaseURL.endsWith('/') ? this.BaseURL : this.BaseURL + '/';

  }

  ngOnInit(): void {
    this.keyword = localStorage.getItem('keyword');
    this.Images = JSON.parse(localStorage.getItem('lastdata'));

    this.route.queryParams.subscribe(async params => {
      const value = params.keyword;
      if (!value) {
        return;
      }

      this.ClearResults();
      const isValid = this.IsValidKeyword(value);
      if (!isValid) {
        return;
      }
      this.keyword = value;
      this.searchInput.nativeElement.value = value;
      await this.SearchApi(value);
    });
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

  async SearchApi(value: string) {
    this.isLoading = true;
    this.noResultFound = false;
    this.changeRef.detectChanges();

    try {

      this.Images = await this.httpclient.get<ImageDto[]>(this.searchUrl + '?keyword=' + value).toPromise();      
      localStorage.setItem('keyword', this.keyword);
      localStorage.setItem('lastdata', JSON.stringify(this.Images));
      if (!this.Images || this.Images.length == 0) {
        this.noResultFound = true;
      }
    }
    catch {
      this.Images = [];
      this.noResultFound = true;
    }
    finally {
      this.isLoading = false;
      this.changeRef.detectChanges();
    }
  }

  onKeyUp(event: KeyboardEvent) {
    if (event.key === this.ENTER_KEY) {
      this.Find((event.target as HTMLInputElement).value);
    }
  }
}
