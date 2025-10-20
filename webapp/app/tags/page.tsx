import { getTags } from "@/lib/actions/tag-actions";
import TagCard from "./TagCard";
import TagPageHeader from "./TagsHeader";

export default async function Page() {
  const {data: tags, error} = await getTags();
  
  if(error) throw error;

  return (
    <div className="w-full px-6">
      <TagPageHeader/>
      <div className="grid grid-cols-3 gap-4">
        {tags?.map(tag => (
          <TagCard tag={tag} key={tag.id}/>
        ))}
      </div>
    </div>
  );
}
