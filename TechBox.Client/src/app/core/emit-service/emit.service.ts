import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { EmitType } from './emit';


@Injectable({
  providedIn: 'root'
})

/**
 * Data Transfering between child to parent component
 */
export class EmitService {
  

  private Subject = new Subject<any>();
  constructor() { }
 
  // set observable new value
  sendChangeEvent(response?: EmitType) {
    this.Subject.next(response);
  }
  // Return observable type
  getChangeEvent(): Observable<EmitType> {
    return this.Subject.asObservable();
  }
}
