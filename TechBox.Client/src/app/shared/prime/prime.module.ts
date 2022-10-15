import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AccordionModule } from 'primeng/accordion'; 
import { MenuItem } from 'primeng/api';

import { PrimeComponent } from './prime.component';



const PrimeNgModules = [
  AccordionModule,
 
];



@NgModule({
  imports: [
    CommonModule,
    PrimeNgModules
  ],
  declarations: [PrimeComponent],
  providers: [],
  exports: PrimeNgModules,
  schemas: [],
})
export class PrimeModule { }
