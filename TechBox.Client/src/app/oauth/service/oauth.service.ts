import { Injectable } from '@angular/core';
import * as Oidc from 'oidc-client';
import { User, UserManager } from 'oidc-client';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OauthService {

  userManager: UserManager;

constructor() {

  const settings = {
    authority: environment.stsBaseUrl,
    client_id: 'techbox-angular',
    redirect_uri: `${environment.clientBaseUrl}/signin-callback`,
    automaticSilentRenew: true,
    silent_redirect_uri: `${environment.clientBaseUrl}/assets/silent-callback.html`,
    post_logout_redirect_uri: `${environment.clientBaseUrl}`,
    response_type: 'code',
    scope: 'openid profile email techbox-api-fullaccess techbox-profile',
    filterProtocolClaims: true,
    loadUserInfo: true,
    userStore: new Oidc.WebStorageStateStore({ store: window.localStorage }),
};
this.userManager = new UserManager(settings);

 }
 public getUser(): Promise<User | null> {
  return this.userManager.getUser();
}

public login(): Promise<void> {
  return this.userManager.signinRedirect({ state: window.location.href });
}

public renewToken(): Promise<User> {
  return this.userManager.signinSilent();
}

public logout(user:User): Promise<void> {
  return this.userManager.signoutRedirect();
}

public finishLogin = (): Promise<User> => {
  return this.userManager.signinRedirectCallback()
      .then(user => {
          return user;
      });
}

}
