/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { OauthGuardService } from './oauth-guard.service';

describe('Service: OauthGuard', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OauthGuardService]
    });
  });

  it('should ...', inject([OauthGuardService], (service: OauthGuardService) => {
    expect(service).toBeTruthy();
  }));
});
