import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './module/components/home/home.component';
import { LayoutComponent } from './module/components/layout/layout.component';
import { OauthGuardService } from './module/guards/oauth-guard.service';
import { SigninCallbackComponent } from './oauth/components/signin-callback/signin-callback.component';

const routes: Routes = [{
  path: '',
  component: LayoutComponent,
  children: [
    {
      path: '',
      children: [
        { path: '', component: HomeComponent,canActivate:[OauthGuardService] },
    
      ]
    }
  ],
},
{ path: 'signin-callback', component: SigninCallbackComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
