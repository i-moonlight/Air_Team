import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Type } from '@angular/core';
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ImageDto } from './models';

describe('AppComponent', () => {
  let httpMock: HttpTestingController;
  let fixture: ComponentFixture<AppComponent>;
  beforeEach(waitForAsync(() => {

    TestBed.configureTestingModule({
      declarations: [
        AppComponent
      ],
      imports: [
        HttpClientTestingModule,
        AppRoutingModule
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    httpMock = fixture.debugElement.injector.get<HttpTestingController>(HttpTestingController as Type<HttpTestingController>);

  }));

  afterEach(() => {
    httpMock.verify();
  });

  it('should create the app', () => {
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it(`should have isLoading 'false'`, () => {
    const app = fixture.componentInstance;
    expect(app.isLoading).toEqual(false);
  });

  it('should render button', () => {
    const compiled = fixture.nativeElement;
    expect(compiled.querySelector('button').textContent).toContain('Search');
  });

  it('should successfull search', async () => {
    const compiled = fixture.nativeElement;
    const app = fixture.componentInstance;

    const dummyImages: ImageDto[] = [
      { imageId: '100', title: 'f14-A' },
      { imageId: '101', title: 'f14-b' },
      { imageId: '102', title: 'f14-c' },
    ];

    const keyword = 'f14';

    var prom = app.SearchApi(keyword);

    const req = httpMock.expectOne(`${app.searchUrl}?keyword=${keyword}`);
    req.flush(dummyImages);

    await prom;
    expect(req.request.method).toBe('GET');
    expect(app.Images).toEqual(dummyImages);
    expect(compiled.querySelectorAll('.image').length).toBe(dummyImages.length);

  });

  it('should prevent search', () => {
    const compiled = fixture.nativeElement;
    const app = fixture.componentInstance;

    const input: HTMLInputElement = compiled.querySelector('input');
    input.value = 'f';

    const button: HTMLButtonElement = compiled.querySelector('button');
    button.click();

    expect(app.Images.length).toEqual(0);

  });

});
