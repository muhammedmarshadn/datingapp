import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { observable, Observable } from 'rxjs';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModalRef :BsModalRef;

  constructor(private modalService:BsModalService) { }

  confirm(title='confirm',message='are you sure u wnt to move on' , 
        btnOkText='ok' , btnCancelText='cancel'): Observable<boolean>
        {
          const config={
            initialState:{
              title,
              message,
              btnOkText,
              btnCancelText

            }
          }

          this.bsModalRef = this.modalService.show(ConfirmDialogComponent,config);
           return new Observable<boolean>(this.getResult());                        // retun the bool result
    }

    private getResult(){
      return (observer) => {
        const subscription = this.bsModalRef.onHidden.subscribe(()=>{      // strng the result of subscription get back from the modal when user asa modal closes
          observer.next(this.bsModalRef.content.result);                  //bool result strng
          observer.complete();                                           //savng
        });

        return{
          unsubscribe(){
            subscription.unsubscribe();
          }

        }
      }
    }

}


