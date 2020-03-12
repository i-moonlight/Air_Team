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

  constructor(private changeRef: ChangeDetectorRef, private httpclient: HttpClient) {
      this.BaseURL= window.Api_URL;

      this.BaseURL = this.BaseURL.endsWith('/') ? this.BaseURL : this.BaseURL + '/';
  }

  ngOnInit(): void {
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
      let url =  this.BaseURL  + 'v1/AirTeam/Search';
      this.httpclient.get<ImageDto[]>(url + '?keyword=' + value).subscribe(data => {
          this.Images = data;
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
