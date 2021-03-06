﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NoteTaker.Models;
using Newtonsoft.Json;
using System.Net;

namespace NoteTaker.Controllers
{
    public class NoteController : Controller
    {
        //
        // GET: /Note/

        public ActionResult Index()
        {
            var notes = GetNotes();
            var tags = GetTags();

            return View(new NoteList()
            {
                 Notes=notes.Items.Select(n=>n.Document),
                 Tags=tags.Items.ToDictionary(x=>x.Key,x=>Int32.Parse(x.Value))
            });
        }

        public ActionResult Tagged(string id)
        {
            var notes = GetNotesByTag(id);
            var tags = GetTags();

            return View("Index",new NoteList()
            {
                Notes = notes.Items.Select(n => n.Document),
                Tags = tags.Items.ToDictionary(x => x.Key, x => Int32.Parse(x.Value))
            });
        }

        public DocumentCollection<Note> GetNotes()
        {
            var json = Couch.Uri.Get("_design/Notes/_view/all?include_docs=true");
            return JsonConvert.DeserializeObject<DocumentCollection<Note>>(json);
        }

        public DocumentCollection<Note> GetNotesByTag(string tag)
        {
            var json = Couch.Uri.Get("_design/Tags/_view/all?key=\""+tag+"\"&reduce=false&include_docs=true");
            return JsonConvert.DeserializeObject<DocumentCollection<Note>>(json);
        }

        public DocumentCollection<object> GetTags()
        {
            var json = Couch.Uri.Get("_design/Tags/_view/all?group_level=1");
            return JsonConvert.DeserializeObject<DocumentCollection<object>>(json);
        }



        public ActionResult Create()
        {

            var note = new Note() { Id = Guid.NewGuid() };
            return View("Edit", note);
        }

        public ActionResult Edit(Guid id)
        {
            var json=Couch.Uri.Get(id);
            return View(JsonConvert.DeserializeObject<Note>(json));
        }

        [HttpPost]
        public ActionResult Edit(Note note)
        {
            try
            {
                Couch.Uri.Put(note.Id, JsonConvert.SerializeObject(note));
                return RedirectToAction("Edit", new { id = note.Id });
            }
            catch (WebException ex)
            {
                var statusCode = ((HttpWebResponse)ex.Response).StatusCode;
                if (statusCode != HttpStatusCode.Conflict)
                    throw;

                ModelState.AddModelError("Revision", "Conflict Detected");
                return View(note);
            }
            
        }

        public ActionResult Delete(Guid id,string rev)
        {
            var json = Couch.Uri.Delete(id,rev);
            return RedirectToAction("Index");
        }


    }
}
