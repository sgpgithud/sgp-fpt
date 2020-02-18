import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PdfJsViewerModule } from 'ng2-pdfjs-viewer';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './page/login/login.component';
import { TopbarComponent } from './component/topbar/topbar.component';
import { SidebarComponent } from './component/sidebar/sidebar.component';
import { HomeComponent } from './page/home/home.component';
import { FooterComponent } from './component/footer/footer.component';
import { ShowpdfComponent } from './page/showpdf/showpdf.component';
import { FptOrderComponent } from './page/fpt-order/fpt-order.component';
import { NgxMaskModule } from 'ngx-mask'
import {DateTimeFormatPipe} from './utils/datetimeformat';
import {DateFormatPipe} from './utils/dateformatpipe';
import { SgpmailerComponent } from './page/tracking/sgpmailer/sgpmailer.component';
import { SgporderComponent } from './page/sgporder/sgporder.component';

export let options = {};

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    TopbarComponent,
    SidebarComponent,
    HomeComponent,
    FooterComponent,
    ShowpdfComponent,
    FptOrderComponent,
    DateTimeFormatPipe,
    DateFormatPipe,
    SgpmailerComponent,
    SgporderComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    PdfJsViewerModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    NgxMaskModule.forRoot(options)
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {

}
