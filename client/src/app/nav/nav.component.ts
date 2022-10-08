import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '../_models/user';

import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model:any={}
  // LoggedIn:boolean;
  // currentUser$:Observable<User>;

  constructor(public accountservice:AccountService) { }

  ngOnInit(): void {
    // this.getCurrentUser();
    // this.currentUser$ = this.accountservice.currentUser$;
  }

  login(){
    //observable is lazy it wont do anything until we subscibe we should subscribe 
    //observables are collection ofmultiple values over time
    this.accountservice.login(this.model).subscribe(Response=>
      {
        console.log(Response);
        // this.LoggedIn = true;
      },error=>{
        console.log(error);
      })
      }


      logout(){
        this.accountservice.logout();
        // this.LoggedIn = false;  
      }


      // getCurrentUser(){                                       //cehck accountservice and get the currentuser
      //   this.accountservice.currentUser$.subscribe(user=>{
      //     this.LoggedIn = !!user;                             // !! change obj into bool
      //   },error=>{
      //     console.log(error);
      //   })
}




