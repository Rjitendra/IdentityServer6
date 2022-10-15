import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { OauthService } from 'src/app/oauth/service/oauth.service';
import { TestService } from 'src/app/oauth/service/test.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  title = 'Angular14';
  public isUserAuthenticated = false;
  public isUserAdmin = false;
  isSignedIn = false;;
  messages: string[] = [];

  currentUser: any;
  constructor(private authService: OauthService, private testApiService: TestService, private httpClient: HttpClient) {
    this.getAsyncGetUserData();
  }

  ngOnInit(): void {

    this.authService.getUser().then(user => {
      this.currentUser = user;

      if (user && user.access_token) {
        this.addMessage('User Logged In');
      } else {
        this.addMessage('User Not Logged In');
      }
    }).catch(err => this.addError(err));
  }

  public onCallAPI() {

    this.clearMessages();

    // const headers = new HttpHeaders()
    //   .set('content-type', 'application/json')
    //   .set('Authorization', 'Bearer ' + this.currentUser.access_token)
    return this.httpClient.get('http://localhost:6001/identity').subscribe(x => {
      this.addMessage('API Result: ' + JSON.stringify(x));
    });
  }

  public login = () => {
    this.authService.login();
  }

  public logout = () => {
    this.authService.logout(this.currentUser);
  }


  get currentUserJson(): string {
    return JSON.stringify(this.currentUser, null, 2);
  }

  async getAsyncGetUserData(): Promise<void> {
    const user = await this.authService.getUser();
    this.currentUser = user;
    if (user) {
      const access_token = user.access_token;
      localStorage.setItem('token', access_token);
      this.addMessage('User Logged In');
    } else {
      this.addMessage('User Not Logged In');

    }

  }


  clearMessages(): void {
    while (this.messages.length) {
      this.messages.pop();
    }
  }

  addMessage(msg: string): void {
    this.messages.push(msg);
  }

  addError(msg: string | any): void {
    this.messages.push('Error: ' + msg && msg.message);
  }

  public async onLogin(): Promise<void> {
    try {
      this.clearMessages();
      await this.authService.login();
    } catch (err) {
      this.addError(err);

    }
  }

  public async onRenewToken(): Promise<void> {
    try {
      this.clearMessages();
      const user = await this.authService.renewToken();
      this.currentUser = user;
      this.addMessage('Silent Renew Success');
    } catch (err) {
      this.addError(err);
    }
  }

  public async onLogout(): Promise<void> {
    this.clearMessages();
    try {
      this.authService.logout(this.currentUser);
    } catch (err) {
      this.addError(err);
    }

  }
}
