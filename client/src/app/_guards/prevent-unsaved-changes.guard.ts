import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanDeactivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';
import { ConfirmService } from '../_services/confirm.service';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<unknown> {           //prevent user exiting without saving changes
  
  constructor (private confirmService:ConfirmService){}

  canDeactivate(component: MemberEditComponent): Observable<boolean> | boolean{
    if(component.editForm.dirty){                                                  //if we edited form
      return this.confirmService.confirm()                                            //modal shows when change of tab
    }
    return true;                                                  //if fromis not dirty 
  }

  // we dont need to subscribe to it becoz inside route guard it will automaticly subscribes
  
}
