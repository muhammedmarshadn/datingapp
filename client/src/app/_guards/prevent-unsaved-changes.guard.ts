import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanDeactivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<unknown> {           //prevent user exiting without saving changes
  canDeactivate(component: MemberEditComponent): boolean{
    if(component.editForm.dirty){
      return confirm('are you sure you want to continue? any unsaved changes will be lost');
    }
    return true;
  }
  
}
