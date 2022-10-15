import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { httpInterceptorProviders } from './interceptor/interceptors';


@NgModule({
    imports: [CommonModule],
    exports: [],
    declarations: [
       

    ],
    providers: [httpInterceptorProviders,
    ],
})
export class CoreModule { }
