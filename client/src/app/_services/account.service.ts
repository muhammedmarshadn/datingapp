import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { pipe, ReplaySubject } from 'rxjs';
import {map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

//this services can be injected into other components or services 
//angular services are singleton(this will stay initialized until our app is disposed off)
//angular components are destroyed as soon as they are NotInUse

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  //= to set prop to something,: to make it type of something
  baseUrl = environment.apiUrl;

  private currentUserSource = new ReplaySubject<User>(1);  //ReplaySubject:like buffer obj ,store value and anytime subscriber subscribe it emits last value in it //(1) is the size of it
  currentUser$ = this.currentUserSource.asObservable();   // $ make it an observbl


  //inject HttpClient into accountservice
  constructor(private http:HttpClient) { }

  login(model:any){
    return this.http.post(this.baseUrl + 'account/login',model).pipe(      //pipe = we get a observeable back from httpget req
      map((response:User)=>{                                               //map = to transform some data in someway or select parts of data 
        const user=response;
        if(user){
          localStorage.setItem('user',JSON.stringify(user));
          this.currentUserSource.next(user);                                    //setting current user getback from the api
        }
      })
    )
  }

  register(model:any){
    return this.http.post(this.baseUrl + 'account/register',model).pipe(
      map((user:User) =>{
        if(user){
          localStorage.setItem('user',JSON.stringify(user));
          this.currentUserSource.next(user);
        }
        return user;
      })
    )
  }

  setCurrentUser(user:User){                //helper method for setting current user,pass in user
    this.currentUserSource.next(user);
  }

logout(){
  localStorage.removeItem('user');
  this.currentUserSource.next(null);      //persisting login
}

}
