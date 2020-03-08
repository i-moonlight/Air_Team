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
  Images: ImageDto[];
  keyword = '';
  isLoading = false;
  readonly url = 'http://localhost:5000/v1/AirTeam/Search';

  constructor(private changeRef: ChangeDetectorRef, private httpclient: HttpClient) {
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
      this.httpclient.get<ImageDto[]>(this.url + '?keyword=' + value).subscribe(data => {
          this.Images = data;
      }, error => {
          this.Images = [];
      }, () => {
          this.isLoading = false;
          this.changeRef.detectChanges();
      });
  }

  onKey(event: any) {
    if (event.keyCode === 13) {
      this.Find(event.target.value);
    }
  }
}
