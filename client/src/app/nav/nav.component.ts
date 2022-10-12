import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
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

  constructor(public accountservice:AccountService,private router:Router,private toastr:ToastrService) { }

  ngOnInit(): void {
    // this.getCurrentUser();
    // this.currentUser$ = this.accountservice.currentUser$;
  }

  login(){
    //observable is lazy it wont do anything until we subscibe we should subscribe 
    //observables are collection ofmultiple values over time
    this.accountservice.login(this.model).subscribe(Response=>
      {
        this.router.navigateByUrl('/members');
        // this.LoggedIn = true;
      })
    }


      logout(){
        this.accountservice.logout();
        this.router.navigateByUrl('/')
        // this.LoggedIn = false;  
      }


      // getCurrentUser(){                                       //cehck accountservice and get the currentuser
      //   this.accountservice.currentUser$.subscribe(user=>{
      //     this.LoggedIn = !!user;                             // !! change obj into bool
      //   },error=>{
      //     console.log(error);
      //   })
}




