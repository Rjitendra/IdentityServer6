import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { User } from 'oidc-client';
import { OauthService } from './oauth.service';

@Injectable({
  providedIn: 'root'
})
export class TestService {


constructor(private httpClient:HttpClient,private authService: OauthService) { }

public callApi(): Promise<any> {
  return this.authService.getUser().then((user: any) => {
    debugger;
    if (user && user.access_token) {
      return this._callApi(user.access_token);
    } else if (user) {
      return this.authService.renewToken().then((user: User) => {
        return this._callApi(user.access_token);
      });
    } else {
      throw new Error('user is not logged in');
    }
  });
}

_callApi(token: string):any {
  const headers = new HttpHeaders({
    Accept: 'application/json',
    Authorization: 'Bearer ' + token
  });

  return this.httpClient.get('https://localhost:6000/api/test', { headers })
    .toPromise();
    // .catch((result: HttpErrorResponse) => {
    //   if (result.status === 401) {
    //     return this.authService.renewToken().then(user => {
    //       return this._callApi(user.access_token);
    //     });
    //   }
    //   throw result;
    // });
}
}