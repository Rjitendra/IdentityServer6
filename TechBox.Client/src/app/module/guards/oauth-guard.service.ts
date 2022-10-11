import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { OauthService } from 'src/app/oauth/service/oauth.service';

@Injectable({
  providedIn: 'root'
})
export class OauthGuardService implements CanActivate {

  constructor(private authService: OauthService, private router: Router,) { }
  canActivate(): Observable<boolean> | Promise<boolean> | boolean {

    return this.getAsyncGetUserData();
  }

  async getAsyncGetUserData(): Promise<boolean> {
  
try {
   const user = await this.authService.getUser();
  //const user = await this.authService.renewToken();
  if (user && user.access_token && !user.expired) {
    return true;
  } else { await this.authService.login();
    return false;}
} catch (error) {
  await this.authService.login();
  return false;
}

 }

  }


