import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl= environment.hubUrl;
  private hubConnection:HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);  // arry of onlineusers ,behaviourSub emit current value whenever suscribe to
  onlineUsers$ = this.onlineUsersSource.asObservable();           //observable 

  constructor(private toastr:ToastrService,private router:Router) { }

  createHubConnection(user:User){                      //calls when a user login or reg
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence',{
        accessTokenFactory:()=>user.token
      })
      .withAutomaticReconnect()
      .build()

      this.hubConnection
        .start()
        .catch(error=>console.log(error))

      this.hubConnection.on('UserIsOnline',username =>{
        this.onlineUsers$.pipe(take(1)).subscribe(usernames =>{     //updating by adding tracking online users
          this.onlineUsersSource.next([...usernames,username])
        })
      })

      this.hubConnection.on('UserIsOffline',username=>{
        this.onlineUsers$.pipe(take(1)).subscribe(usernames =>{
          this.onlineUsersSource.next([...usernames.filter(x=>x !== username)])     // removing  
        })
      })

      this.hubConnection.on("GetOnlineUsers",(usernames:string[])=>{
        this.onlineUsersSource.next(usernames);                         // strng current online users to arry(behavioursub)
      })

      this.hubConnection.on('NewMessageReceived',({username,knownAs}) =>{
        this.toastr.info(knownAs + 'has sent a new msg !')                //toasting
        .onTap                                                            //on clicking the toast
        .pipe(take(1))
        .subscribe(()=> this.router.navigateByUrl('/members/'+ username +'?tab=3')) // route to msg send by the user
      })
  }

  stopHubConnection(){                                 //calls when user logs out
    this.hubConnection.stop().catch(error=>console.log(error));
  }
}
