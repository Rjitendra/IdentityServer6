/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { OauthService } from './oauth.service';

describe('Service: Oauth', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OauthService]
    });
  });

  it('should ...', inject([OauthService], (service: OauthService) => {
    expect(service).toBeTruthy();
  }));
});
