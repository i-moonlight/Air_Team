import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Type } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { ImageDto } from './models';

describe('AppComponent', () => {
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [
        AppComponent
      ],
      imports: [
        HttpClientTestingModule
      ]
    }).compileComponents();
  }));

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it(`should have isLoading 'false'`, () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app.isLoading).toEqual(false);
  });

  it('should render button', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement;
    expect(compiled.querySelector('button').textContent).toContain('Search');
  });

  it('should successfull search', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const compiled = fixture.nativeElement;
    const app = fixture.componentInstance;

    const httpMock = fixture.debugElement.injector.get<HttpTestingController>(HttpTestingController as Type<HttpTestingController>);

    const dummyImages: ImageDto[] = [
      { imageId: '100', title: 'f14-A', baseImageUrl: 'image/notfound.jpg'  },
      { imageId: '101', title: 'f14-b', baseImageUrl: 'image/notfound.jpg'  },
      { imageId: '102', title: 'f14-c', baseImageUrl: 'image/notfound.jpg'  },
    ];

    const keyword = 'f14';
    const input: HTMLInputElement = compiled.querySelector('input');
    input.value = keyword;

    const button: HTMLButtonElement = compiled.querySelector('button');
    button.click();

    const req = httpMock.expectOne(`${app.url}?keyword=${keyword}`);
    req.flush(dummyImages);

    expect(req.request.method).toBe('GET');
    expect(app.Images).toEqual(dummyImages);
    fixture.detectChanges();

    expect(compiled.querySelectorAll('.image').length).toBe(dummyImages.length);

  });

  it('should prevent search', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const compiled = fixture.nativeElement;
    const app = fixture.componentInstance;

    const input: HTMLInputElement = compiled.querySelector('input');
    input.value = 'f';

    const button: HTMLButtonElement = compiled.querySelector('button');
    button.click();

    expect(app.keyword).toBe(input.value);
    expect(app.Images.length).toEqual(0);

  });

});
