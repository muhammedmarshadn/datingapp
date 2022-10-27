export interface Group{
    name:string;
    connections:connection[];
}

interface connection{
    connectionId : string;
    username:string;
}