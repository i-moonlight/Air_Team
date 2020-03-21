import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
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

  get searchApi(){
    return this.BaseURL + 'v1/AirTeam/Search';
  }

  constructor(private changeRef: ChangeDetectorRef, private httpclient: HttpClient) {
      this.BaseURL= window.Api_URL;

      this.BaseURL = this.BaseURL.endsWith('/') ? this.BaseURL : this.BaseURL + '/';
  }

  ngOnInit(): void {
    this.keyword = localStorage.getItem('keyword');
    this.Images = JSON.parse(localStorage.getItem('lastdata'));
  }

  Find(value: string): void {
      this.keyword = value;
      const isValid = this.keyword && this.keyword.trim().length > 2;
      if (!isValid) {
        this.Images = [];
        this.changeRef.detectChanges();
        return;
      }

      this.isLoading = true;      
      this.httpclient.get<ImageDto[]>(this.searchApi + '?keyword=' + value)
      .subscribe(data => {
          this.Images = data;
          localStorage.setItem('keyword', this.keyword);
          localStorage.setItem('lastdata', JSON.stringify(this.Images));
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
