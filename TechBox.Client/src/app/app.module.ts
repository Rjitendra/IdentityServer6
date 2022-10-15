import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { LayoutComponent } from './module/components/layout/layout.component';
import { SigninCallbackComponent } from './oauth/components/signin-callback/signin-callback.component';
import { HomeComponent } from './module/components/home/home.component';
import { PrimeModule } from './shared/prime/prime.module';
import { CoreModule } from './core/core.module';


@NgModule({
  declarations: [
    AppComponent,
    SigninCallbackComponent,
    LayoutComponent,
    HomeComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    CoreModule,
    PrimeModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
