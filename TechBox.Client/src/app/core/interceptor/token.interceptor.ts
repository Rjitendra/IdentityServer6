import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { User } from 'oidc-client';
import { switchMap } from 'rxjs/operators';
import { OauthService } from 'src/app/oauth/service/oauth.service';


@Injectable({
    providedIn: 'root'
})
export class TokenInterceptor implements HttpInterceptor {
    constructor(private oauthService: OauthService) { }

    /**
     * Adding Bearer Token to HttpRequest header
     */
    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {


      
        return from(this.getCurrentUserValue())
            .pipe(
                switchMap((token: User) => {
                    const headers = request.headers
                        .set('Authorization', 'Bearer ' + token.access_token)
                        .append('Content-Type', 'application/json');
                    const requestClone = request.clone({
                        headers
                    });
                    return next.handle(requestClone);
                })
            );
    }
    
    async getCurrentUserValue(): Promise<any> {

        try {
            let user: User | null = await this.oauthService.getUser();
            if (user && user.access_token && !user.expired) {
                 return user;
            } else if (user && !user.expired) {
                user = await this.oauthService.renewToken()
                 return user;

            } else { this.oauthService.login()}
        } catch (error) {
            this.oauthService.login()
        }
 }
}
