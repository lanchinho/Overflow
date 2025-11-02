"use server";

import { auth } from "@/auth";
import { fetchClient } from "../fetchClient";
import { User } from "next-auth";

export async function testAuth(){
  return fetchClient<string>("/test/auth", "GET");
}

export async function getCurrentUser(): Promise<User | null>{
  const session = await auth();
  return session?.user ?? null;
}