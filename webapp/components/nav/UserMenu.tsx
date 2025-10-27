"use client";

import { Dropdown, DropdownItem, DropdownMenu, DropdownTrigger } from "@heroui/dropdown";
import { Avatar } from "@heroui/react";
import { User } from "next-auth";
import { signOut } from "next-auth/react";

type Props ={
    user: User;
}

export default function UserMenu({user} : Props) {
  return (
    <Dropdown>
      <DropdownTrigger>
        <div className="flex items-center gap-2 cursor-pointer">
          <Avatar suppressHydrationWarning
            color='secondary' size='sm' name={user.displayName?.charAt(0).toUpperCase()}/>
          {user.displayName}
        </div>            
      </DropdownTrigger>
      <DropdownMenu>
        <DropdownItem key='edit'>Edit Profile</DropdownItem>
        <DropdownItem
          key='logot'
          className="text-danger"
          color ='danger'
          onClick={()=>  signOut({redirectTo: "/"})}
        >Sign out</DropdownItem>
      </DropdownMenu>
    </Dropdown>
  );
}
