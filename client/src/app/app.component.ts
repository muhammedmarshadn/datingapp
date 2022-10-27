import { HttpClient } from '@angular/common/http';
import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';
import { Component, OnInit } from '@angular/core';
// import { error } from 'console';
import { Subscriber } from 'rxjs';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';
import { PresenceService } from './_services/presence.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  title = 'The Dating App';
  users:any;

  constructor(private accountService:AccountService,private presence:PresenceService){}
 // constructor(private http: HttpClient,private accountService:AccountService){}


  ngOnInit() {
    // this.GetUsers();
    this.setCurrentUser();
}

setCurrentUser(){                                               //take a look inside brwsr lclstrg and see if we got a key or obj with key of user
  const user:User = JSON.parse(localStorage.getItem('user'));   //bcoz we stringified obj in loclstrg,getout of stringfyd from into user obj user:User
  if(user){                                                       //checking if we hav a user
    this.accountService.setCurrentUser(user);                      //seting current user
    this.presence.createHubConnection(user);                       // starting hubconnection
  }
}

 
// GetUsers(){
  
//   this.http.get('https://localhost:5001/api/users').subscribe(response =>{
//     this.users = response;
//   },error => {
//     console.log(error);
//   })
// }
}
