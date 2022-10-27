import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Group } from '../_models/group';
import { Message } from '../_models/message';
import { User } from '../_models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubconnection:HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http:HttpClient) { }

  createHubConnection(user: User, otherUsername: string){       //strting hub connection
    this.hubconnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername,{
        accessTokenFactory:()=>user.token
      })
      .withAutomaticReconnect()
      .build()

      this.hubconnection.start().catch(error=>console.log(error));

      this.hubconnection.on("RecieveMessageThread",messages =>{
        this.messageThreadSource.next(messages);                    //geting the messages
      })

      this.hubconnection.on("NewMessage",message=>{
        this.messageThread$.pipe(take(1)).subscribe(messages=>{  //updating the message[] with the message just recived
          this.messageThreadSource.next([...messages,message])  //create a new[] & populate behavrsubjct //to get msg without mutaion we use ... opertor
        })                                                     //the spread operator allows us to copy all elements from the existing array or object into another array or object.
      })

      this.hubconnection.on("UpdatedGroup",(group:Group)=>{
        if(group.connections.some(x=>x.username === otherUsername)){  // if usernames are equal
          this.messageThread$.pipe(take(1)).subscribe(messages=>{      // in that message[]
            messages.forEach(message=>{                                 // each message
              if(!message.dateRead){                                     // is unread?? then
                message.dateRead = new Date(Date.now())                  // seting new read time
              }
            })
            this.messageThreadSource.next([...messages]);               // new msg []
          })
        }
      })
  }

  stopHubConnection(){
     if(this.hubconnection){                            // checking if there is a hubconnection or not to avoid exception
      this.hubconnection.stop();

    }
  }

  getMessages(pageNumber,pageSize,container){
    let params = getPaginationHeaders(pageNumber,pageSize);
    params = params.append('container',container);
    return getPaginatedResult<Message[]>(this.baseUrl+'messages',params,this.http);

  }

  getMessageThread(username:string){
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }

  async sendMessage(username:string,content:string){           //async garuntee it will return a promise
    return this.hubconnection.invoke('SendMessage',{recipientUsername:username,content})   // invoke - A Promise that resolves with the result of the server method (if any), or rejects with an error.
      .catch(error=>console.log(error));
  }

  deleteMessage(id: number){
    return this.http.delete(this.baseUrl + 'messages/'+id);
  }
}
