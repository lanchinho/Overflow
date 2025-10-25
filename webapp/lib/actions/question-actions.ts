"use server";

import { revalidatePath } from "next/cache";
import { fetchClient } from "../fetchClient";
import { AnswerSchema } from "../schemas/answerSchema";
import { QuestionSchema } from "../schemas/questionSchema";
import { Answer, Question } from "../types";

export async function getQuestions(tag?: string){
  let url = "/questions";    
  if (tag) url += "?tag=" + tag;  
  return fetchClient<Question[]>(url, "GET");
}

export async function getQuestionById(id: string){
  const url = `/questions/${id}`;     
  return fetchClient<Question>(url, "GET");
}

export async function searchQuestions(query: string){
  return fetchClient<Question[]>(`/search?query=${query}`, "GET");
}

export async function postQuestion(question: QuestionSchema){
  return fetchClient<Question>("/questions", "POST", {body: question});
}

export async function updateQuestion(question: QuestionSchema, id:string){
  return fetchClient<Question>(`/questions/${id}`, "PUT", {body: question});
}

export async function deleteQuestion(id: string){
  return fetchClient<Question>(`/questions/${id}`, "DELETE");
}

export async function postAnswer(data: AnswerSchema, questionId: string){
  const result = await fetchClient<Answer>(`/questions/${questionId}/answers`, "POST", {body: data});
  revalidatePath(`/questions/${questionId}`);
  return result;
}